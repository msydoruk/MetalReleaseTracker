using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;
using OpenQA.Selenium;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers;

public class BlackMetalVendorParser : IListingParser, IAlbumDetailParser
{
    private static readonly AlbumMediaType[] CategoryMediaTypes =
        [AlbumMediaType.CD, AlbumMediaType.LP, AlbumMediaType.Tape];

    private static readonly string[] CatalogueUrls =
    [
        "https://black-metal-vendor.com/en/Audio-Records-A-Z/Compact-Disc:::2_122.html",
        "https://black-metal-vendor.com/en/Audio-Records-A-Z/Vinyl:::2_123.html",
        "https://black-metal-vendor.com/en/Audio-Records-A-Z/Audio-Tape:::2_121.html"
    ];

    private readonly ISeleniumWebDriverFactory _webDriverFactory;
    private readonly ILogger<BlackMetalVendorParser> _logger;
    private Queue<(string Url, AlbumMediaType MediaType)> _pendingCategoryUrls = new();
    private AlbumMediaType? _currentCategoryMediaType;
    private bool _categoryQueueInitialized;

    public BlackMetalVendorParser(
        ISeleniumWebDriverFactory webDriverFactory,
        ILogger<BlackMetalVendorParser> logger)
    {
        _webDriverFactory = webDriverFactory;
        _logger = logger;
    }

    public DistributorCode DistributorCode => DistributorCode.BlackMetalVendor;

    public async Task<ListingPageResult> ParseListingsAsync(string url, CancellationToken cancellationToken)
    {
        IWebDriver? driver = null;
        try
        {
            var pageUrl = url;

            if (!_categoryQueueInitialized)
            {
                pageUrl = InitializeCategoryQueue(url);
                _categoryQueueInitialized = true;
            }

            _logger.LogInformation("Crawling BlackMetalVendor page: {Url}.", pageUrl);

            driver = _webDriverFactory.CreateDriver();
            var htmlDocument = await LoadPageWithSelenium(driver, pageUrl, cancellationToken);
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
        catch (BlackMetalVendorParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during BlackMetalVendor catalogue crawl for URL: {Url}.", url);
            throw new BlackMetalVendorParserException($"Failed to crawl BlackMetalVendor catalogue: {url}", exception);
        }
        finally
        {
            DisposeDriver(driver);
        }
    }

    public async Task<AlbumParsedEvent> ParseAlbumDetailAsync(string detailUrl, CancellationToken cancellationToken)
    {
        IWebDriver? driver = null;
        try
        {
            driver = _webDriverFactory.CreateDriver();
            _logger.LogInformation("Created Selenium WebDriver for BlackMetalVendor detail parse.");

            var htmlDocument = await LoadPageWithSelenium(driver, detailUrl, cancellationToken);
            return ParseAlbumFromDetailPage(htmlDocument, detailUrl);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (BlackMetalVendorParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during BlackMetalVendor detail parse for URL: {Url}.", detailUrl);
            throw new BlackMetalVendorParserException($"Failed to parse BlackMetalVendor detail page: {detailUrl}", exception);
        }
        finally
        {
            DisposeDriver(driver);
        }
    }

    private string InitializeCategoryQueue(string initialUrl)
    {
        var entries = new List<(string Url, AlbumMediaType MediaType)>();

        for (var i = 0; i < CatalogueUrls.Length; i++)
        {
            var mediaType = i < CategoryMediaTypes.Length ? CategoryMediaTypes[i] : AlbumMediaType.CD;

            if (string.Equals(CatalogueUrls[i], initialUrl, StringComparison.OrdinalIgnoreCase))
            {
                _currentCategoryMediaType = mediaType;
            }
            else
            {
                entries.Add((CatalogueUrls[i], mediaType));
            }
        }

        _pendingCategoryUrls = new Queue<(string Url, AlbumMediaType MediaType)>(entries);

        _logger.LogInformation(
            "Initialized BlackMetalVendor category queue. Starting with {Url}, {Remaining} categories remaining.",
            initialUrl,
            _pendingCategoryUrls.Count);

        return initialUrl;
    }

    private string? ResolveNextPageUrl(HtmlDocument htmlDocument)
    {
        var nextPageInCategory = GetNextPageUrl(htmlDocument);

        if (!string.IsNullOrEmpty(nextPageInCategory))
        {
            return nextPageInCategory;
        }

        if (_pendingCategoryUrls.Count > 0)
        {
            var (nextUrl, mediaType) = _pendingCategoryUrls.Dequeue();
            _currentCategoryMediaType = mediaType;
            _logger.LogInformation("Moving to next category: {Url}.", nextUrl);
            return nextUrl;
        }

        _logger.LogInformation("All BlackMetalVendor categories crawled.");
        return null;
    }

    private string? GetNextPageUrl(HtmlDocument htmlDocument)
    {
        var nextPageLink = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalVendorSelectors.NextPageLink);

        if (nextPageLink != null)
        {
            var nextUrl = HtmlEntity.DeEntitize(nextPageLink.GetAttributeValue("href", string.Empty).Trim());
            if (!string.IsNullOrEmpty(nextUrl))
            {
                return nextUrl;
            }
        }

        return null;
    }

    private List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();

        var listingBoxes = htmlDocument.DocumentNode.SelectNodes(BlackMetalVendorSelectors.ListingBoxes);
        if (listingBoxes == null)
        {
            return results;
        }

        foreach (var box in listingBoxes)
        {
            var listing = ParseListingFromBox(box);
            if (listing != null)
            {
                results.Add(listing);
            }
        }

        return results;
    }

    private ListingItem? ParseListingFromBox(HtmlNode box)
    {
        var titleLink = box.SelectSingleNode(BlackMetalVendorSelectors.ListingTitleLink);
        if (titleLink == null)
        {
            return null;
        }

        var href = HtmlEntity.DeEntitize(titleLink.GetAttributeValue("href", string.Empty).Trim());
        var titleText = HtmlEntity.DeEntitize(titleLink.InnerText?.Trim() ?? string.Empty);

        if (string.IsNullOrEmpty(href) || string.IsNullOrEmpty(titleText))
        {
            return null;
        }

        var (bandName, albumName, _) = SplitTitle(titleText);

        return new ListingItem(bandName, albumName, href, titleText, _currentCategoryMediaType);
    }

    private AlbumParsedEvent ParseAlbumFromDetailPage(HtmlDocument htmlDocument, string detailUrl)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalVendorSelectors.DetailTitle)
            ?? htmlDocument.DocumentNode.SelectSingleNode(BlackMetalVendorSelectors.DetailTitleFallback);
        var titleText = titleNode != null
            ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
            : string.Empty;

        var (bandName, albumName, _) = SplitTitle(titleText);
        var sku = ParseSku(detailUrl);
        var price = ParsePriceFromPage(htmlDocument);
        var photoUrl = ParsePhotoUrlFromPage(htmlDocument);

        return new AlbumParsedEvent
        {
            DistributorCode = DistributorCode,
            BandName = bandName,
            SKU = sku,
            Name = albumName,
            ReleaseDate = DateTime.MinValue,
            Genre = string.Empty,
            Price = price,
            PurchaseUrl = detailUrl,
            PhotoUrl = photoUrl,
            Label = string.Empty,
            Press = sku,
            Description = string.Empty,
            Status = null
        };
    }

    private async Task<HtmlDocument> LoadPageWithSelenium(IWebDriver driver, string url, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        await Task.Run(() => driver.Navigate().GoToUrl(url), cancellationToken);
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

        var pageSource = driver.PageSource;

        if (string.IsNullOrEmpty(pageSource))
        {
            var error = $"Failed to load page via Selenium: {url}";
            _logger.LogError(error);
            throw new BlackMetalVendorParserException(error);
        }

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(pageSource);

        if (htmlDocument.DocumentNode == null)
        {
            var error = $"Failed to parse HTML document from Selenium page source: {url}";
            _logger.LogError(error);
            throw new BlackMetalVendorParserException(error);
        }

        return htmlDocument;
    }

    private void DisposeDriver(IWebDriver? driver)
    {
        if (driver != null)
        {
            try
            {
                driver.Quit();
                driver.Dispose();
                _logger.LogInformation("Disposed Selenium WebDriver for BlackMetalVendor.");
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Error disposing Selenium WebDriver.");
            }
        }
    }

    private static (string BandName, string AlbumName, string MediaTypeRaw) SplitTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return (string.Empty, string.Empty, string.Empty);
        }

        var mediaTypeRaw = string.Empty;
        var mediaMatch = Regex.Match(title, @"\(([^)]+)\)\s*$");
        if (mediaMatch.Success)
        {
            mediaTypeRaw = mediaMatch.Groups[1].Value.Trim();
            title = title[..mediaMatch.Index].Trim();
        }

        char[] separators = ['\u2013', '\u2014', '-'];
        foreach (var separator in separators)
        {
            var separatorWithSpaces = $" {separator} ";
            var parts = title.Split(new[] { separatorWithSpaces }, 2, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                return (parts[0].Trim(), parts[1].Trim(), mediaTypeRaw);
            }
        }

        return (title, title, mediaTypeRaw);
    }

    private static string ParseSku(string albumUrl)
    {
        var idMatch = Regex.Match(albumUrl, @"::(\d+)\.html");
        if (idMatch.Success)
        {
            return idMatch.Groups[1].Value;
        }

        var pidMatch = Regex.Match(albumUrl, @"products_id=(\d+)");
        if (pidMatch.Success)
        {
            return pidMatch.Groups[1].Value;
        }

        return albumUrl;
    }

    private static float ParsePriceFromPage(HtmlDocument htmlDocument)
    {
        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalVendorSelectors.DetailPrice)
            ?? htmlDocument.DocumentNode.SelectSingleNode(BlackMetalVendorSelectors.DetailPriceFallback);

        if (priceNode != null)
        {
            var priceText = HtmlEntity.DeEntitize(priceNode.InnerText?.Trim() ?? string.Empty);
            var match = Regex.Match(priceText, @"[\d]+[.,][\d]+");
            if (match.Success)
            {
                return AlbumParsingHelper.ParsePrice(match.Value.Replace(',', '.'));
            }
        }

        return 0.0f;
    }

    private static string ParsePhotoUrlFromPage(HtmlDocument htmlDocument)
    {
        var imgNode = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalVendorSelectors.DetailPhoto)
            ?? htmlDocument.DocumentNode.SelectSingleNode(BlackMetalVendorSelectors.DetailPhotoFallback);

        if (imgNode != null)
        {
            var src = imgNode.GetAttributeValue("src", null)
                ?? imgNode.GetAttributeValue("data-src", null);
            if (!string.IsNullOrEmpty(src))
            {
                src = src.Replace("thumbnail_images", "popup_images")
                         .Replace("midi_images", "popup_images")
                         .Replace("mini_images", "popup_images");
                return src;
            }
        }

        return string.Empty;
    }
}
