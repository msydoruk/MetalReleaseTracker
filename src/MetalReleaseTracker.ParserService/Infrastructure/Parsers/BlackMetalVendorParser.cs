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

public class BlackMetalVendorParser : IParser
{
    private readonly ISeleniumWebDriverFactory _webDriverFactory;
    private readonly ILogger<BlackMetalVendorParser> _logger;

    public DistributorCode DistributorCode => DistributorCode.BlackMetalVendor;

    public BlackMetalVendorParser(
        ISeleniumWebDriverFactory webDriverFactory,
        ILogger<BlackMetalVendorParser> logger)
    {
        _webDriverFactory = webDriverFactory;
        _logger = logger;
    }

    public async Task<PageParsedResult> ParseAsync(string parsingUrl, CancellationToken cancellationToken)
    {
        IWebDriver? driver = null;
        try
        {
            driver = _webDriverFactory.CreateDriver();
            _logger.LogInformation("Created Selenium WebDriver for BlackMetalVendor parsing.");

            var htmlDocument = await LoadPageWithSelenium(driver, parsingUrl, cancellationToken);
            var parsedAlbums = ParseAlbumsFromListing(htmlDocument, cancellationToken);
            var nextPageUrl = GetNextPageUrl(htmlDocument, parsingUrl);

            _logger.LogInformation($"Parsed {parsedAlbums.Count} albums from listing page.");

            return new PageParsedResult
            {
                ParsedAlbums = parsedAlbums,
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
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during BlackMetalVendor parsing for URL: {parsingUrl}");
            throw new BlackMetalVendorParserException($"Failed to parse BlackMetalVendor page: {parsingUrl}", ex);
        }
        finally
        {
            if (driver != null)
            {
                try
                {
                    driver.Quit();
                    driver.Dispose();
                    _logger.LogInformation("Disposed Selenium WebDriver for BlackMetalVendor.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disposing Selenium WebDriver.");
                }
            }
        }
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

    private List<AlbumParsedEvent> ParseAlbumsFromListing(HtmlDocument htmlDocument, CancellationToken cancellationToken)
    {
        var results = new List<AlbumParsedEvent>();

        var listingBoxes = htmlDocument.DocumentNode.SelectNodes("//div[@class='listingbox']");
        if (listingBoxes == null)
        {
            _logger.LogWarning("No listing boxes found on page.");
            return results;
        }

        foreach (var box in listingBoxes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var album = ParseAlbumFromListingBox(box);
            if (album != null)
            {
                results.Add(album);
            }
        }

        return results;
    }

    private AlbumParsedEvent? ParseAlbumFromListingBox(HtmlNode box)
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

        var (bandName, albumName, mediaTypeRaw) = SplitTitle(titleText);
        var sku = ParseSku(href);
        var price = ParsePriceFromListing(box);
        var photoUrl = ParsePhotoUrlFromListing(box);
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
            PurchaseUrl = href,
            PhotoUrl = photoUrl,
            Media = media,
            Label = string.Empty,
            Press = sku,
            Description = string.Empty,
            Status = null
        };
    }

    private (string BandName, string AlbumName, string MediaTypeRaw) SplitTitle(string title)
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

        char[] separators = { '\u2013', '\u2014', '-' };
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

    private string ParseSku(string albumUrl)
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

    private float ParsePriceFromListing(HtmlNode box)
    {
        var priceNode = box.SelectSingleNode(".//span[@class='value_price']");
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

    private string ParsePhotoUrlFromListing(HtmlNode box)
    {
        var imgNode = box.SelectSingleNode(".//div[contains(@class,'prod_image')]//img");
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

    private AlbumMediaType? ParseMediaType(string mediaTypeRaw)
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

    private string? GetNextPageUrl(HtmlDocument htmlDocument, string currentUrl)
    {
        var currentPage = 1;
        var pageMatch = Regex.Match(currentUrl, @"[&?]page=(\d+)");
        if (pageMatch.Success)
        {
            currentPage = int.Parse(pageMatch.Groups[1].Value);
        }

        var nextPage = currentPage + 1;

        var nextPageNode = htmlDocument.DocumentNode.SelectSingleNode(
            $"//a[@class='pageResults' and contains(@href, 'page={nextPage}')]");

        if (nextPageNode != null)
        {
            var nextPageUrl = nextPageNode.GetAttributeValue("href", null);
            if (!string.IsNullOrEmpty(nextPageUrl))
            {
                nextPageUrl = HtmlEntity.DeEntitize(nextPageUrl);
                _logger.LogInformation($"Next page found: {nextPageUrl}.");
                return nextPageUrl;
            }
        }

        _logger.LogInformation("Next page not found.");
        return null;
    }
}
