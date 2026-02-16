using System.Globalization;
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

public class SeasonOfMistParser : IListingParser, IAlbumDetailParser
{
    private static readonly string[] CategoryUrls =
    [
        "https://shop.season-of-mist.com/music?cat=3",
        "https://shop.season-of-mist.com/music?cat=5",
        "https://shop.season-of-mist.com/music?cat=23"
    ];

    private readonly IHtmlDocumentLoader _htmlDocumentLoader;
    private readonly GeneralParserSettings _generalParserSettings;
    private readonly ILogger<SeasonOfMistParser> _logger;
    private readonly Random _random = new();
    private Queue<string> _pendingCategoryUrls = new();
    private bool _categoryQueueInitialized;

    public SeasonOfMistParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<SeasonOfMistParser> logger)
    {
        _htmlDocumentLoader = htmlDocumentLoader;
        _generalParserSettings = generalParserSettings.Value;
        _logger = logger;
    }

    public DistributorCode DistributorCode => DistributorCode.SeasonOfMist;

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

            _logger.LogInformation("Crawling SeasonOfMist page: {Url}.", pageUrl);

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
        catch (SeasonOfMistParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during SeasonOfMist catalogue crawl for URL: {Url}.", url);
            throw new SeasonOfMistParserException($"Failed to crawl SeasonOfMist catalogue: {url}", exception);
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
        catch (SeasonOfMistParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during SeasonOfMist detail parse for URL: {Url}.", detailUrl);
            throw new SeasonOfMistParserException($"Failed to parse SeasonOfMist detail page: {detailUrl}", exception);
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
            "Initialized SeasonOfMist category queue. Starting with {Url}, {Remaining} categories remaining.",
            initialUrl,
            _pendingCategoryUrls.Count);

        return initialUrl;
    }

    private string? ResolveNextPageUrl(HtmlDocument htmlDocument)
    {
        var nextPageLink = htmlDocument.DocumentNode.SelectSingleNode(
            "//a[contains(@class,'next') or @title='Next']");

        if (nextPageLink != null)
        {
            var nextUrl = HtmlEntity.DeEntitize(nextPageLink.GetAttributeValue("href", string.Empty).Trim());
            if (!string.IsNullOrEmpty(nextUrl))
            {
                return nextUrl;
            }
        }

        if (_pendingCategoryUrls.Count > 0)
        {
            var nextCategory = _pendingCategoryUrls.Dequeue();
            _logger.LogInformation("Moving to next category: {Url}.", nextCategory);
            return nextCategory;
        }

        _logger.LogInformation("All SeasonOfMist categories crawled.");
        return null;
    }

    private List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productItems = htmlDocument.DocumentNode.SelectNodes(
            "//div[contains(@class,'products-grid')]//div[contains(@class,'item')]");
        if (productItems == null)
        {
            return results;
        }

        foreach (var item in productItems)
        {
            var nameLink = item.SelectSingleNode(".//h2[contains(@class,'product-name')]/a");
            if (nameLink == null)
            {
                continue;
            }

            var href = HtmlEntity.DeEntitize(nameLink.GetAttributeValue("href", string.Empty).Trim());
            if (string.IsNullOrEmpty(href) || !processedUrls.Add(href))
            {
                continue;
            }

            var rawTitle = HtmlEntity.DeEntitize(nameLink.InnerText?.Trim() ?? string.Empty);
            if (string.IsNullOrEmpty(rawTitle))
            {
                continue;
            }

            var (bandName, albumTitle) = ParseProductName(rawTitle);

            results.Add(new ListingItem(bandName, albumTitle, href, rawTitle));
        }

        return results;
    }

    private async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);

        var bandName = ParseAttributeValue(htmlDocument, "Band");
        var albumName = ParseAttributeValue(htmlDocument, "Title");
        var sku = ParseAttributeValue(htmlDocument, "Catalog #");
        var price = ParsePrice(htmlDocument);
        var photoUrl = ParsePhotoUrl(htmlDocument);
        var genre = ParseGenre(htmlDocument);
        var releaseDate = ParseReleaseDate(htmlDocument);
        var label = ParseLabel(htmlDocument);
        var media = InferMediaType(detailUrl);
        var status = ParseStatus(htmlDocument);

        return new AlbumParsedEvent
        {
            DistributorCode = DistributorCode,
            BandName = bandName,
            SKU = sku,
            Name = albumName,
            ReleaseDate = releaseDate,
            Genre = genre,
            Price = price,
            PurchaseUrl = detailUrl,
            PhotoUrl = photoUrl,
            Media = media,
            Label = label,
            Press = sku,
            Description = string.Empty,
            Status = status
        };
    }

    private string ParseAttributeValue(HtmlDocument htmlDocument, string attributeName)
    {
        var thNode = htmlDocument.DocumentNode.SelectSingleNode(
            $"//table[@id='product-attribute-specs-table']//th[normalize-space(text())='{attributeName}']");

        if (thNode != null)
        {
            var tdNode = thNode.SelectSingleNode("following-sibling::td");
            if (tdNode != null)
            {
                var text = HtmlEntity.DeEntitize(tdNode.InnerText?.Trim() ?? string.Empty);
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }
        }

        return string.Empty;
    }

    private float ParsePrice(HtmlDocument htmlDocument)
    {
        var priceNode = htmlDocument.DocumentNode.SelectSingleNode("//span[contains(@class,'price')]");
        if (priceNode != null)
        {
            var priceText = HtmlEntity.DeEntitize(priceNode.InnerText?.Trim() ?? string.Empty)
                .Replace("€", string.Empty)
                .Replace(",", ".")
                .Trim();

            return AlbumParsingHelper.ParsePrice(priceText);
        }

        return 0.0f;
    }

    private string ParsePhotoUrl(HtmlDocument htmlDocument)
    {
        var imgNode = htmlDocument.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'product-img-box')]//img")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//a[contains(@class,'product-image')]//img");

        if (imgNode != null)
        {
            var src = imgNode.GetAttributeValue("src", string.Empty);
            if (!string.IsNullOrEmpty(src))
            {
                return src;
            }
        }

        return string.Empty;
    }

    private string ParseGenre(HtmlDocument htmlDocument)
    {
        var genre = ParseAttributeValue(htmlDocument, "Detailed musical style");
        if (!string.IsNullOrEmpty(genre))
        {
            return genre;
        }

        return ParseAttributeValue(htmlDocument, "Generic musical style");
    }

    private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
    {
        var dateText = ParseAttributeValue(htmlDocument, "Release Date");
        if (string.IsNullOrEmpty(dateText))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParseExact(dateText, "d MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTime.TryParseExact(dateText, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        if (DateTime.TryParseExact(dateText, "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        return DateTime.MinValue;
    }

    private string ParseLabel(HtmlDocument htmlDocument)
    {
        var thNode = htmlDocument.DocumentNode.SelectSingleNode(
            "//table[@id='product-attribute-specs-table']//th[normalize-space(text())='Label']");

        if (thNode != null)
        {
            var tdNode = thNode.SelectSingleNode("following-sibling::td");
            if (tdNode != null)
            {
                var text = HtmlEntity.DeEntitize(tdNode.InnerText?.Trim() ?? string.Empty);
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }
        }

        return string.Empty;
    }

    private AlbumStatus? ParseStatus(HtmlDocument htmlDocument)
    {
        var buttonNode = htmlDocument.DocumentNode.SelectSingleNode(
            "//button[contains(@class,'btn-cart')]")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//button[@type='submit' and contains(@class,'add')]");

        if (buttonNode != null)
        {
            var buttonText = HtmlEntity.DeEntitize(buttonNode.InnerText?.Trim() ?? string.Empty);
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
            throw new SeasonOfMistParserException(error);
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

    private static (string BandName, string AlbumTitle) ParseProductName(string rawTitle)
    {
        var parts = rawTitle.Split(" - ");
        if (parts.Length >= 3)
        {
            var bandName = parts[0].Trim();
            var albumTitle = string.Join(" - ", parts[1..^1]).Trim();
            return (bandName, albumTitle);
        }

        if (parts.Length == 2)
        {
            return (parts[0].Trim(), parts[1].Trim());
        }

        return (string.Empty, rawTitle.Trim());
    }

    private static AlbumMediaType? InferMediaType(string detailUrl)
    {
        var slug = detailUrl.ToUpperInvariant();

        if (slug.Contains("-LP") || slug.Contains("-VINYL"))
        {
            return AlbumMediaType.LP;
        }

        if (slug.Contains("-CASSETTE") || slug.Contains("-TAPE"))
        {
            return AlbumMediaType.Tape;
        }

        if (slug.Contains("-CD"))
        {
            return AlbumMediaType.CD;
        }

        return null;
    }
}
