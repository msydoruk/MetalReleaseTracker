using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers;

public class ParagonRecordsParser : IListingParser, IAlbumDetailParser
{
    private const string BaseUrl = "https://www.paragonrecords.org";

    private static readonly string[] CategoryUrls =
    [
        "https://www.paragonrecords.org/collections/cd",
        "https://www.paragonrecords.org/collections/vinyl",
        "https://www.paragonrecords.org/collections/cassette"
    ];

    private static readonly string[] FormatTokens =
    [
        "CASSETTE", "DIGIPAK", "DIGISLEEVE", "GATEFOLD", "DOUBLE", "DIGI",
        "COLOURED", "COLORED", "DLP", "LP", "DCD", "CD", "EP", "TAPE"
    ];

    private readonly IHtmlDocumentLoader _htmlDocumentLoader;
    private readonly GeneralParserSettings _generalParserSettings;
    private readonly ILogger<ParagonRecordsParser> _logger;
    private readonly Random _random = new();
    private Queue<string> _pendingCategoryUrls = new();
    private bool _categoryQueueInitialized;

    public ParagonRecordsParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<ParagonRecordsParser> logger)
    {
        _htmlDocumentLoader = htmlDocumentLoader;
        _generalParserSettings = generalParserSettings.Value;
        _logger = logger;
    }

    public DistributorCode DistributorCode => DistributorCode.ParagonRecords;

    public async Task<ListingPageResult> ParseListingsAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var pageUrl = url;

            if (!_categoryQueueInitialized)
            {
                pageUrl = InitializeCategoryQueue(url);
                _categoryQueueInitialized = true;
            }

            _logger.LogInformation("Crawling ParagonRecords page: {Url}.", pageUrl);

            var htmlDocument = await LoadHtmlDocument(pageUrl, cancellationToken);
            var listings = ParseListingsFromPage(htmlDocument);

            _logger.LogInformation("Parsed {Count} products from page.", listings.Count);

            var nextPageUrl = ResolveNextPageUrl(htmlDocument);

            return new ListingPageResult
            {
                Listings = listings,
                NextPageUrl = nextPageUrl
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ParagonRecordsParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during ParagonRecords catalogue crawl for URL: {Url}.", url);
            throw new ParagonRecordsParserException($"Failed to crawl ParagonRecords catalogue: {url}", exception);
        }
    }

    public async Task<AlbumParsedEvent> ParseAlbumDetailAsync(string detailUrl, CancellationToken cancellationToken)
    {
        try
        {
            await DelayBetweenRequests(cancellationToken);
            return await ParseAlbumDetails(detailUrl, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (ParagonRecordsParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during ParagonRecords detail parse for URL: {Url}.", detailUrl);
            throw new ParagonRecordsParserException($"Failed to parse ParagonRecords detail page: {detailUrl}", exception);
        }
    }

    private string InitializeCategoryQueue(string initialUrl)
    {
        var urls = new List<string>();

        foreach (var categoryUrl in CategoryUrls)
        {
            if (!string.Equals(categoryUrl, initialUrl, StringComparison.OrdinalIgnoreCase))
            {
                urls.Add(categoryUrl);
            }
        }

        _pendingCategoryUrls = new Queue<string>(urls);

        _logger.LogInformation(
            "Initialized ParagonRecords category queue. Starting with {Url}, {Remaining} categories remaining.",
            initialUrl,
            _pendingCategoryUrls.Count);

        return initialUrl;
    }

    private string? ResolveNextPageUrl(HtmlDocument htmlDocument)
    {
        var nextPageLink = htmlDocument.DocumentNode.SelectSingleNode(
            "//ul[contains(@class,'pagination')]//a[.//span[contains(text(),'Next')]]");

        if (nextPageLink != null)
        {
            var href = HtmlEntity.DeEntitize(nextPageLink.GetAttributeValue("href", string.Empty).Trim());
            if (!string.IsNullOrEmpty(href))
            {
                return ToAbsoluteUrl(href);
            }
        }

        if (_pendingCategoryUrls.Count > 0)
        {
            var nextCategory = _pendingCategoryUrls.Dequeue();
            _logger.LogInformation("Moving to next category: {Url}.", nextCategory);
            return nextCategory;
        }

        _logger.LogInformation("All ParagonRecords categories crawled.");
        return null;
    }

    private List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productGrid = htmlDocument.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'grid--view-items')]");
        if (productGrid == null)
        {
            return results;
        }

        var productItems = productGrid.SelectNodes(".//div[contains(@class,'grid__item')]");
        if (productItems == null)
        {
            return results;
        }

        foreach (var item in productItems)
        {
            var linkNode = item.SelectSingleNode(".//a[contains(@class,'grid-view-item__link')]");
            if (linkNode == null)
            {
                continue;
            }

            var href = HtmlEntity.DeEntitize(linkNode.GetAttributeValue("href", string.Empty).Trim());
            if (string.IsNullOrEmpty(href))
            {
                continue;
            }

            var absoluteUrl = ToAbsoluteUrl(href);
            if (!processedUrls.Add(absoluteUrl))
            {
                continue;
            }

            var titleNode = item.SelectSingleNode(".//div[contains(@class,'grid-view-item__title')]");
            var rawTitle = titleNode != null
                ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
                : string.Empty;

            if (string.IsNullOrEmpty(rawTitle))
            {
                continue;
            }

            var (bandName, albumTitle) = ParseProductName(rawTitle);

            results.Add(new ListingItem(bandName, albumTitle, absoluteUrl, rawTitle));
        }

        return results;
    }

    private async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);
        var pageSource = htmlDocument.DocumentNode.OuterHtml;

        var titleText = ParseTitle(htmlDocument);
        var (bandName, albumName) = ParseProductName(titleText);
        var price = ParsePrice(htmlDocument);
        var photoUrl = ParsePhotoUrl(htmlDocument);
        var genre = ParseGenre(htmlDocument);
        var label = ParseLabel(pageSource);
        var media = InferMediaType(detailUrl, titleText);
        var status = ParseStatus(htmlDocument);

        return new AlbumParsedEvent
        {
            DistributorCode = DistributorCode,
            BandName = bandName,
            SKU = string.Empty,
            Name = albumName,
            ReleaseDate = DateTime.MinValue,
            Genre = genre,
            Price = price,
            PurchaseUrl = detailUrl,
            PhotoUrl = photoUrl,
            Media = media,
            Label = label,
            Press = string.Empty,
            Description = string.Empty,
            Status = status
        };
    }

    private string ParseTitle(HtmlDocument htmlDocument)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode(
            "//h1[contains(@class,'product-single__title')]")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//h1");

        return titleNode != null
            ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
            : string.Empty;
    }

    private float ParsePrice(HtmlDocument htmlDocument)
    {
        var ogPrice = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:price:amount']");
        if (ogPrice != null)
        {
            var priceText = ogPrice.GetAttributeValue("content", string.Empty);
            if (!string.IsNullOrEmpty(priceText))
            {
                return AlbumParsingHelper.ParsePrice(priceText);
            }
        }

        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(
            "//span[contains(@class,'product-price__price')]");
        if (priceNode != null)
        {
            var priceText = HtmlEntity.DeEntitize(priceNode.InnerText?.Trim() ?? string.Empty)
                .Replace("$", string.Empty)
                .Trim();

            return AlbumParsingHelper.ParsePrice(priceText);
        }

        return 0.0f;
    }

    private string ParsePhotoUrl(HtmlDocument htmlDocument)
    {
        var ogImage = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image:secure_url']");
        if (ogImage != null)
        {
            var content = ogImage.GetAttributeValue("content", string.Empty);
            if (!string.IsNullOrEmpty(content))
            {
                return content;
            }
        }

        ogImage = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
        if (ogImage != null)
        {
            var content = ogImage.GetAttributeValue("content", string.Empty);
            if (!string.IsNullOrEmpty(content))
            {
                return content.Replace("http://", "https://");
            }
        }

        return string.Empty;
    }

    private string ParseGenre(HtmlDocument htmlDocument)
    {
        var descNode = htmlDocument.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'product-single__description')]");

        if (descNode != null)
        {
            var text = HtmlEntity.DeEntitize(descNode.InnerText?.Trim() ?? string.Empty);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        return string.Empty;
    }

    private AlbumStatus? ParseStatus(HtmlDocument htmlDocument)
    {
        var buttonSpan = htmlDocument.DocumentNode.SelectSingleNode(
            "//span[@id='AddToCartText-product-template']")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//button[contains(@class,'product-form__cart-submit')]");

        if (buttonSpan != null)
        {
            var buttonText = HtmlEntity.DeEntitize(buttonSpan.InnerText?.Trim() ?? string.Empty);
            if (buttonText.Contains("Pre-Order", StringComparison.OrdinalIgnoreCase) ||
                buttonText.Contains("Preorder", StringComparison.OrdinalIgnoreCase))
            {
                return AlbumStatus.PreOrder;
            }
        }

        return null;
    }

    private async Task<HtmlDocument> LoadHtmlDocument(string url, CancellationToken cancellationToken)
    {
        var htmlDocument = await _htmlDocumentLoader.LoadHtmlDocumentAsync(url, cancellationToken);

        if (htmlDocument?.DocumentNode == null)
        {
            var error = $"Failed to load or parse the HTML document {url}.";
            _logger.LogError(error);
            throw new ParagonRecordsParserException(error);
        }

        return htmlDocument;
    }

    private async Task DelayBetweenRequests(CancellationToken cancellationToken)
    {
        var delaySeconds = _random.Next(
            _generalParserSettings.MinDelayBetweenRequestsSeconds,
            _generalParserSettings.MaxDelayBetweenRequestsSeconds);

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
    }

    private static string ToAbsoluteUrl(string href)
    {
        if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return href;
        }

        return BaseUrl + href;
    }

    private static (string BandName, string AlbumTitle) ParseProductName(string rawTitle)
    {
        var parts = rawTitle.Split(" - ", 2);
        if (parts.Length == 2)
        {
            var bandName = parts[0].Trim();
            var albumWithFormat = parts[1].Trim();
            var albumTitle = StripFormatSuffix(albumWithFormat);
            return (bandName, albumTitle);
        }

        return (string.Empty, rawTitle.Trim());
    }

    private static string StripFormatSuffix(string albumText)
    {
        var result = albumText.TrimEnd();
        bool changed;

        do
        {
            changed = false;
            foreach (var token in FormatTokens)
            {
                if (result.EndsWith($" {token}", StringComparison.OrdinalIgnoreCase))
                {
                    result = result[..^(token.Length + 1)].TrimEnd();
                    changed = true;
                    break;
                }
            }
        }
        while (changed && result.Length > 0);

        return result.Length > 0 ? result : albumText.TrimEnd();
    }

    private static string ParseLabel(string pageSource)
    {
        var match = Regex.Match(
            pageSource,
            @"""product"":\{[^}]*""vendor"":""([^""]+)""");

        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static AlbumMediaType? InferMediaType(string detailUrl, string title)
    {
        var slug = detailUrl.ToUpperInvariant();

        if (slug.Contains("-LP") || slug.Contains("-VINYL") || slug.Contains("-DLP"))
        {
            return AlbumMediaType.LP;
        }

        if (slug.Contains("-CASSETTE") || slug.Contains("-TAPE"))
        {
            return AlbumMediaType.Tape;
        }

        if (slug.Contains("-CD") || slug.Contains("-DCD"))
        {
            return AlbumMediaType.CD;
        }

        var upper = title.ToUpperInvariant();

        if (upper.EndsWith(" LP") || upper.Contains(" LP ") || upper.Contains(" VINYL") || upper.Contains(" DLP"))
        {
            return AlbumMediaType.LP;
        }

        if (upper.EndsWith(" CASSETTE") || upper.Contains(" CASSETTE ") || upper.Contains(" TAPE"))
        {
            return AlbumMediaType.Tape;
        }

        if (upper.EndsWith(" CD") || upper.Contains(" CD ") || upper.Contains(" DCD"))
        {
            return AlbumMediaType.CD;
        }

        return null;
    }
}
