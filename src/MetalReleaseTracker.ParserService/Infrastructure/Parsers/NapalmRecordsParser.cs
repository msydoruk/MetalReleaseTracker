using System.Globalization;
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

public class NapalmRecordsParser : IListingParser, IAlbumDetailParser
{
    private static readonly string[] CategoryUrls =
    [
        "https://napalmrecords.com/english/music/cds?product_list_dir=desc&product_list_order=release_date",
        "https://napalmrecords.com/english/music/lps?product_list_dir=desc&product_list_order=release_date",
        "https://napalmrecords.com/english/music/tapes?product_list_dir=desc&product_list_order=release_date"
    ];

    private readonly IHtmlDocumentLoader _htmlDocumentLoader;
    private readonly GeneralParserSettings _generalParserSettings;
    private readonly ILogger<NapalmRecordsParser> _logger;
    private readonly Random _random = new();
    private Queue<string> _pendingCategoryUrls = new();
    private bool _categoryQueueInitialized;

    public NapalmRecordsParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<NapalmRecordsParser> logger)
    {
        _htmlDocumentLoader = htmlDocumentLoader;
        _generalParserSettings = generalParserSettings.Value;
        _logger = logger;
    }

    public DistributorCode DistributorCode => DistributorCode.NapalmRecords;

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

            _logger.LogInformation("Crawling NapalmRecords page: {Url}.", pageUrl);

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
        catch (NapalmRecordsParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during NapalmRecords catalogue crawl for URL: {Url}.", url);
            throw new NapalmRecordsParserException($"Failed to crawl NapalmRecords catalogue: {url}", exception);
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
        catch (NapalmRecordsParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during NapalmRecords detail parse for URL: {Url}.", detailUrl);
            throw new NapalmRecordsParserException($"Failed to parse NapalmRecords detail page: {detailUrl}", exception);
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
            "Initialized NapalmRecords category queue. Starting with {Url}, {Remaining} categories remaining.",
            initialUrl,
            _pendingCategoryUrls.Count);

        return initialUrl;
    }

    private string? ResolveNextPageUrl(HtmlDocument htmlDocument)
    {
        var nextPageLink = htmlDocument.DocumentNode.SelectSingleNode(
            "//a[contains(@class,'action') and contains(@class,'next')]");

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

        _logger.LogInformation("All NapalmRecords categories crawled.");
        return null;
    }

    private List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productItems = htmlDocument.DocumentNode.SelectNodes("//li[contains(@class,'product-item')]");
        if (productItems == null)
        {
            return results;
        }

        foreach (var item in productItems)
        {
            var linkNode = item.SelectSingleNode(".//a[contains(@class,'product-item-link')]");
            if (linkNode == null)
            {
                continue;
            }

            var href = HtmlEntity.DeEntitize(linkNode.GetAttributeValue("href", string.Empty).Trim());
            if (string.IsNullOrEmpty(href) || !processedUrls.Add(href))
            {
                continue;
            }

            var albumTitle = HtmlEntity.DeEntitize(linkNode.InnerText?.Trim() ?? string.Empty);
            if (string.IsNullOrEmpty(albumTitle))
            {
                continue;
            }

            var bandNameNode = item.SelectSingleNode(".//div[contains(@class,'custom-band-name')]");
            var bandName = bandNameNode != null
                ? HtmlEntity.DeEntitize(bandNameNode.InnerText?.Trim() ?? string.Empty)
                : string.Empty;

            var rawTitle = !string.IsNullOrEmpty(bandName) ? $"{bandName} - {albumTitle}" : albumTitle;

            results.Add(new ListingItem(bandName, albumTitle, href, rawTitle));
        }

        return results;
    }

    private async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);
        var pageSource = htmlDocument.DocumentNode.OuterHtml;

        var bandName = ParseAttribute(htmlDocument, "Band");
        var albumName = ParseAlbumName(htmlDocument);
        var sku = ParseSku(htmlDocument);
        var price = ParsePrice(pageSource, htmlDocument);
        var photoUrl = ParsePhotoUrl(htmlDocument);
        var genre = ParseAttribute(htmlDocument, "Genre");
        var releaseDate = ParseReleaseDate(htmlDocument);
        var media = InferMediaType(detailUrl, albumName);
        var description = ParseDescription(htmlDocument);

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
            Label = "Napalm Records",
            Press = sku,
            Description = description,
            Status = null
        };
    }

    private string ParseAlbumName(HtmlDocument htmlDocument)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//h1[contains(@class,'page-title')]")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//h1");

        return titleNode != null
            ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
            : string.Empty;
    }

    private string ParseAttribute(HtmlDocument htmlDocument, string attributeName)
    {
        var cell = htmlDocument.DocumentNode.SelectSingleNode(
            $"//table[@id='product-attribute-specs-table']//td[@data-th='{attributeName}']");

        if (cell != null)
        {
            var text = HtmlEntity.DeEntitize(cell.InnerText?.Trim() ?? string.Empty);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        return string.Empty;
    }

    private string ParseSku(HtmlDocument htmlDocument)
    {
        var strongNodes = htmlDocument.DocumentNode.SelectNodes("//strong");
        if (strongNodes != null)
        {
            foreach (var strong in strongNodes)
            {
                var text = strong.InnerText?.Trim() ?? string.Empty;
                if (text.Contains("Art", StringComparison.OrdinalIgnoreCase) &&
                    text.Contains("Nr", StringComparison.OrdinalIgnoreCase))
                {
                    var parentText = strong.ParentNode?.InnerText?.Trim() ?? string.Empty;
                    var match = Regex.Match(parentText, @"(\d+)");
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
        }

        var formNode = htmlDocument.DocumentNode.SelectSingleNode("//form[@data-product-sku]");
        if (formNode != null)
        {
            var sku = formNode.GetAttributeValue("data-product-sku", string.Empty);
            if (!string.IsNullOrEmpty(sku))
            {
                return sku;
            }
        }

        return string.Empty;
    }

    private float ParsePrice(string pageSource, HtmlDocument htmlDocument)
    {
        var jsMatch = Regex.Match(pageSource, @"""final_price""[:\s]*""?([\d.]+)""?");
        if (jsMatch.Success)
        {
            return AlbumParsingHelper.ParsePrice(jsMatch.Groups[1].Value);
        }

        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(
            "//span[contains(@class,'price-wrapper')]");
        if (priceNode != null)
        {
            var priceAmount = priceNode.GetAttributeValue("data-price-amount", string.Empty);
            if (!string.IsNullOrEmpty(priceAmount))
            {
                return AlbumParsingHelper.ParsePrice(priceAmount);
            }
        }

        return 0.0f;
    }

    private string ParsePhotoUrl(HtmlDocument htmlDocument)
    {
        var ogImage = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='og:image']");
        if (ogImage != null)
        {
            var content = ogImage.GetAttributeValue("content", string.Empty);
            if (!string.IsNullOrEmpty(content))
            {
                return content;
            }
        }

        var galleryImg = htmlDocument.DocumentNode.SelectSingleNode(
            "//img[contains(@class,'product-image-photo') and contains(@src,'/media/catalog/product/')]");
        if (galleryImg != null)
        {
            var src = galleryImg.GetAttributeValue("src", string.Empty);
            if (!string.IsNullOrEmpty(src))
            {
                return src;
            }
        }

        return string.Empty;
    }

    private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
    {
        var dateText = ParseAttribute(htmlDocument, "Release Date");
        if (string.IsNullOrEmpty(dateText))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParseExact(dateText, "MMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTime.TryParseExact(dateText, "MMMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        var yearMatch = Regex.Match(dateText, @"\d{4}");
        if (yearMatch.Success)
        {
            return AlbumParsingHelper.ParseYear(yearMatch.Value);
        }

        return DateTime.MinValue;
    }

    private string ParseDescription(HtmlDocument htmlDocument)
    {
        var descNode = htmlDocument.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'description')]//div[@class='value']");

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

    private async Task<HtmlDocument> LoadHtmlDocument(string url, CancellationToken cancellationToken)
    {
        var htmlDocument = await _htmlDocumentLoader.LoadHtmlDocumentAsync(url, cancellationToken);

        if (htmlDocument?.DocumentNode == null)
        {
            var error = $"Failed to load or parse the HTML document {url}.";
            _logger.LogError(error);
            throw new NapalmRecordsParserException(error);
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

    private static AlbumMediaType? InferMediaType(string detailUrl, string albumName)
    {
        var combined = $"{detailUrl} {albumName}".ToUpper();

        if (combined.Contains("/LPS") || combined.Contains("- LP") || combined.Contains("VINYL"))
        {
            return AlbumMediaType.LP;
        }

        if (combined.Contains("/TAPES") || combined.Contains("TAPE") || combined.Contains("CASSETTE"))
        {
            return AlbumMediaType.Tape;
        }

        if (combined.Contains("/CDS") || combined.Contains("- CD") || combined.Contains("DIGIPAK"))
        {
            return AlbumMediaType.CD;
        }

        return null;
    }
}
