using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using OpenQA.Selenium;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers;

public class BlackMetalVendorParser : IListingParser, IAlbumDetailParser
{
    private readonly IHtmlDocumentLoader _htmlDocumentLoader;
    private readonly ISeleniumWebDriverFactory _webDriverFactory;
    private readonly ILogger<BlackMetalVendorParser> _logger;
    private Queue<string> _pendingSubcategoryUrls = new();

    public DistributorCode DistributorCode => DistributorCode.BlackMetalVendor;

    public BlackMetalVendorParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        ISeleniumWebDriverFactory webDriverFactory,
        ILogger<BlackMetalVendorParser> logger)
    {
        _htmlDocumentLoader = htmlDocumentLoader;
        _webDriverFactory = webDriverFactory;
        _logger = logger;
    }

    public async Task<ListingPageResult> ParseListingsAsync(string url, CancellationToken cancellationToken)
    {
        try
        {
            var pageUrl = url;

            if (IsMainCategoryPage(url))
            {
                pageUrl = await DiscoverSubcategoriesAsync(url, cancellationToken);
                if (pageUrl == null)
                {
                    return new ListingPageResult { Listings = [], NextPageUrl = null };
                }
            }

            _logger.LogInformation("Crawling BlackMetalVendor page: {Url}.", pageUrl);

            var htmlDocument = await _htmlDocumentLoader.LoadHtmlDocumentAsync(pageUrl, cancellationToken);
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

    private async Task<string?> DiscoverSubcategoriesAsync(string mainPageUrl, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading BlackMetalVendor main catalogue page: {Url}.", mainPageUrl);

        var mainPage = await _htmlDocumentLoader.LoadHtmlDocumentAsync(mainPageUrl, cancellationToken);
        var subcategoryUrls = ExtractSubcategoryUrls(mainPage);

        _logger.LogInformation("Found {Count} subcategories to crawl.", subcategoryUrls.Count);

        _pendingSubcategoryUrls = new Queue<string>(subcategoryUrls);

        return _pendingSubcategoryUrls.Count > 0
            ? _pendingSubcategoryUrls.Dequeue()
            : null;
    }

    private string? ResolveNextPageUrl(HtmlDocument htmlDocument)
    {
        var nextPageInSubcategory = GetNextPageUrl(htmlDocument);

        if (!string.IsNullOrEmpty(nextPageInSubcategory))
        {
            return nextPageInSubcategory;
        }

        if (_pendingSubcategoryUrls.Count > 0)
        {
            var nextSubcategory = _pendingSubcategoryUrls.Dequeue();
            _logger.LogInformation("Moving to next subcategory: {Url}.", nextSubcategory);
            return nextSubcategory;
        }

        _logger.LogInformation("All subcategories crawled.");
        return null;
    }

    private List<string> ExtractSubcategoryUrls(HtmlDocument htmlDocument)
    {
        var subcategoryUrls = new List<string>();
        var categoryLinks = htmlDocument.DocumentNode.SelectNodes(
            "//a[contains(@href, ':::2_')]");

        if (categoryLinks == null)
        {
            _logger.LogWarning("No subcategory links found matching ':::2_' pattern.");
            return subcategoryUrls;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var link in categoryLinks)
        {
            var href = HtmlEntity.DeEntitize(link.GetAttributeValue("href", string.Empty).Trim());

            if (string.IsNullOrEmpty(href) || !seen.Add(href))
            {
                continue;
            }

            if (href.Contains("/Audio-Tape:::") ||
                href.Contains("/Compact-Disc:::") ||
                href.Contains("/Vinyl:::"))
            {
                subcategoryUrls.Add(href);
                _logger.LogInformation("Found subcategory: {Url}.", href);
            }
        }

        return subcategoryUrls;
    }

    private string? GetNextPageUrl(HtmlDocument htmlDocument)
    {
        var nextPageLink = htmlDocument.DocumentNode.SelectSingleNode(
            "//a[@class='pageResults' and contains(@title, 'chste Seite')]");

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

        var listingBoxes = htmlDocument.DocumentNode.SelectNodes("//div[@class='listingbox']");
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
        var titleLink = box.SelectSingleNode(".//div[@class='lb_title']//h2//a");
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

        return new ListingItem(bandName, albumName, href, titleText);
    }

    private AlbumParsedEvent ParseAlbumFromDetailPage(HtmlDocument htmlDocument, string detailUrl)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='lb_title']//h2//a")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//h1");
        var titleText = titleNode != null
            ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
            : string.Empty;

        var (bandName, albumName, mediaTypeRaw) = SplitTitle(titleText);
        var sku = ParseSku(detailUrl);
        var price = ParsePriceFromPage(htmlDocument);
        var photoUrl = ParsePhotoUrlFromPage(htmlDocument);
        var media = ParseMediaType(mediaTypeRaw);

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
            Media = media,
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

    private static bool IsMainCategoryPage(string url)
    {
        return !url.Contains(":::2_");
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
        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(".//span[@class='value_price']")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//span[contains(@class,'price')]");

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
        var imgNode = htmlDocument.DocumentNode.SelectSingleNode(".//div[contains(@class,'prod_image')]//img")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//img[contains(@class,'product')]");

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

    private static AlbumMediaType? ParseMediaType(string mediaTypeRaw)
    {
        if (string.IsNullOrWhiteSpace(mediaTypeRaw))
        {
            return null;
        }

        var upper = mediaTypeRaw.ToUpper();

        if (upper.Contains("LP") || upper.Contains("VINYL"))
        {
            return AlbumMediaType.LP;
        }

        if (upper.Contains("CD") || upper.Contains("MCD"))
        {
            return AlbumMediaType.CD;
        }

        if (upper.Contains("TAPE") || upper.Contains("MC") || upper.Contains("CS") || upper.Contains("CASSETTE"))
        {
            return AlbumMediaType.Tape;
        }

        return null;
    }
}
