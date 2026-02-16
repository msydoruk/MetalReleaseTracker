using System.Text.Json;
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

public class BlackMetalStoreParser : IListingParser, IAlbumDetailParser
{
    private static readonly string[] CategoryUrls =
    [
        "https://blackmetalstore.com/categoria-produto/cds/",
        "https://blackmetalstore.com/categoria-produto/cassettes/",
        "https://blackmetalstore.com/categoria-produto/vinyl/"
    ];

    private static readonly char[] TitleSeparators = ['\u2013', '\u2014', '-'];

    private readonly IHtmlDocumentLoader _htmlDocumentLoader;
    private readonly GeneralParserSettings _generalParserSettings;
    private readonly ILogger<BlackMetalStoreParser> _logger;
    private readonly Random _random = new();
    private Queue<string> _pendingCategoryUrls = new();
    private bool _categoryQueueInitialized;

    public BlackMetalStoreParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<BlackMetalStoreParser> logger)
    {
        _htmlDocumentLoader = htmlDocumentLoader;
        _generalParserSettings = generalParserSettings.Value;
        _logger = logger;
    }

    public DistributorCode DistributorCode => DistributorCode.BlackMetalStore;

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

            _logger.LogInformation("Crawling BlackMetalStore page: {Url}.", pageUrl);

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
        catch (BlackMetalStoreParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during BlackMetalStore catalogue crawl for URL: {Url}.", url);
            throw new BlackMetalStoreParserException($"Failed to crawl BlackMetalStore catalogue: {url}", exception);
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
        catch (BlackMetalStoreParserException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error during BlackMetalStore detail parse for URL: {Url}.", detailUrl);
            throw new BlackMetalStoreParserException($"Failed to parse BlackMetalStore detail page: {detailUrl}", exception);
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
            "Initialized BlackMetalStore category queue. Starting with {Url}, {Remaining} categories remaining.",
            initialUrl,
            _pendingCategoryUrls.Count);

        return initialUrl;
    }

    private string? ResolveNextPageUrl(HtmlDocument htmlDocument)
    {
        var nextPageLink = htmlDocument.DocumentNode.SelectSingleNode(
            "//a[contains(@class,'next') and contains(@class,'page-numbers')]");

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

        _logger.LogInformation("All BlackMetalStore categories crawled.");
        return null;
    }

    private List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productLinks = htmlDocument.DocumentNode.SelectNodes("//a[contains(@href,'/produto/')]");
        if (productLinks == null)
        {
            return results;
        }

        foreach (var linkNode in productLinks)
        {
            var href = HtmlEntity.DeEntitize(linkNode.GetAttributeValue("href", string.Empty).Trim());
            if (string.IsNullOrEmpty(href) || !processedUrls.Add(href))
            {
                continue;
            }

            var titleNode = FindTitleForProduct(linkNode);
            var titleText = titleNode != null
                ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
                : string.Empty;

            if (string.IsNullOrEmpty(titleText))
            {
                continue;
            }

            var (bandName, albumTitle, _) = SplitTitle(titleText);
            results.Add(new ListingItem(bandName, albumTitle, href, titleText));
        }

        return results;
    }

    private async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);
        var jsonLd = ExtractJsonLd(htmlDocument);

        var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//h1");
        var titleText = titleNode != null
            ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
            : string.Empty;

        var (bandName, albumName, mediaTypeRaw) = SplitTitle(titleText);
        var sku = ParseSku(jsonLd, htmlDocument, detailUrl);
        var price = ParsePrice(jsonLd, htmlDocument);
        var photoUrl = ParsePhotoUrl(jsonLd, htmlDocument);
        var media = ParseMediaType(mediaTypeRaw);
        var label = ParseLabel(htmlDocument);
        var genre = ParseGenre(jsonLd, htmlDocument);
        var description = ParseDescription(jsonLd, htmlDocument);

        return new AlbumParsedEvent
        {
            DistributorCode = DistributorCode,
            BandName = bandName,
            SKU = sku,
            Name = albumName,
            ReleaseDate = DateTime.MinValue,
            Genre = genre,
            Price = price,
            PurchaseUrl = detailUrl,
            PhotoUrl = photoUrl,
            Media = media,
            Label = label,
            Press = sku,
            Description = description,
            Status = null
        };
    }

    private JsonElement? ExtractJsonLd(HtmlDocument htmlDocument)
    {
        var scriptNodes = htmlDocument.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
        if (scriptNodes == null)
        {
            return null;
        }

        foreach (var scriptNode in scriptNodes)
        {
            var json = scriptNode.InnerText?.Trim();
            if (string.IsNullOrEmpty(json))
            {
                continue;
            }

            try
            {
                var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                if (root.TryGetProperty("@type", out var typeElement) &&
                    typeElement.GetString() == "Product")
                {
                    return root;
                }

                if (root.TryGetProperty("@graph", out var graph) &&
                    graph.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in graph.EnumerateArray())
                    {
                        if (item.TryGetProperty("@type", out var itemType) &&
                            itemType.GetString() == "Product")
                        {
                            return item;
                        }
                    }
                }
            }
            catch (JsonException)
            {
            }
        }

        return null;
    }

    private string ParseSku(JsonElement? jsonLd, HtmlDocument htmlDocument, string detailUrl)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("sku", out var skuElement))
        {
            var skuValue = skuElement.ValueKind == JsonValueKind.Number
                ? skuElement.GetInt64().ToString()
                : skuElement.GetString();

            if (!string.IsNullOrEmpty(skuValue))
            {
                return skuValue;
            }
        }

        var skuNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='sku']");
        var sku = skuNode?.InnerText?.Trim();

        if (!string.IsNullOrEmpty(sku))
        {
            return sku;
        }

        var slugMatch = Regex.Match(detailUrl, @"/produto/[^/]+/([^/]+)/?$");
        return slugMatch.Success ? slugMatch.Groups[1].Value : detailUrl;
    }

    private float ParsePrice(JsonElement? jsonLd, HtmlDocument htmlDocument)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("offers", out var offersElement))
        {
            var offer = offersElement.ValueKind == JsonValueKind.Array && offersElement.GetArrayLength() > 0
                ? offersElement[0]
                : offersElement;

            var priceStr = ExtractPriceFromOffer(offer);
            if (!string.IsNullOrEmpty(priceStr))
            {
                return AlbumParsingHelper.ParsePrice(priceStr);
            }
        }

        var priceNode = htmlDocument.DocumentNode.SelectSingleNode("//p[contains(@class,'price')]//bdi")
            ?? htmlDocument.DocumentNode.SelectSingleNode("//span[contains(@class,'woocommerce-Price-amount')]//bdi");

        var priceText = priceNode?.InnerText?.Trim();
        if (string.IsNullOrEmpty(priceText))
        {
            return 0.0f;
        }

        priceText = HtmlEntity.DeEntitize(priceText);
        var match = Regex.Match(priceText, @"[\d]+[.,][\d]+");

        return match.Success ? AlbumParsingHelper.ParsePrice(match.Value.Replace(',', '.')) : 0.0f;
    }

    private string ParsePhotoUrl(JsonElement? jsonLd, HtmlDocument htmlDocument)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("image", out var imageElement))
        {
            var imageUrl = imageElement.ValueKind == JsonValueKind.Array
                ? imageElement[0].GetString()
                : imageElement.GetString();

            if (!string.IsNullOrEmpty(imageUrl))
            {
                return imageUrl;
            }
        }

        var imgNode = htmlDocument.DocumentNode.SelectSingleNode("//img[contains(@class,'wp-post-image')]");
        if (imgNode != null)
        {
            var src = imgNode.GetAttributeValue("data-src", null)
                ?? imgNode.GetAttributeValue("src", null);

            if (!string.IsNullOrEmpty(src) && !src.Contains("placeholder"))
            {
                return src;
            }
        }

        return string.Empty;
    }

    private string ParseLabel(HtmlDocument htmlDocument)
    {
        var categoryLinks = htmlDocument.DocumentNode.SelectNodes("//span[@class='posted_in']//a");
        if (categoryLinks == null || categoryLinks.Count < 2)
        {
            return string.Empty;
        }

        var formatCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CDs", "Cassettes", "VINYL", "Vinyl"
        };

        foreach (var link in categoryLinks)
        {
            var text = HtmlEntity.DeEntitize(link.InnerText?.Trim() ?? string.Empty);
            if (!string.IsNullOrEmpty(text) && !formatCategories.Contains(text))
            {
                return text;
            }
        }

        return string.Empty;
    }

    private string ParseGenre(JsonElement? jsonLd, HtmlDocument htmlDocument)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("brand", out var brandElement))
        {
            var brandName = ExtractBrandName(brandElement);
            if (!string.IsNullOrEmpty(brandName))
            {
                return brandName;
            }
        }

        var brandNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class,'product-brand')]//a");
        if (brandNode != null)
        {
            var text = HtmlEntity.DeEntitize(brandNode.InnerText?.Trim() ?? string.Empty);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        return string.Empty;
    }

    private string ParseDescription(JsonElement? jsonLd, HtmlDocument htmlDocument)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("description", out var descElement))
        {
            var description = descElement.GetString();
            if (!string.IsNullOrEmpty(description))
            {
                return StripHtml(description);
            }
        }

        var descNode = htmlDocument.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'woocommerce-product-details__short-description')]");

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
            throw new BlackMetalStoreParserException(error);
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

    private static HtmlNode? FindTitleForProduct(HtmlNode linkNode)
    {
        var parent = linkNode.ParentNode;
        while (parent != null)
        {
            if (parent.HasClass("product"))
            {
                return parent.SelectSingleNode(".//h2[contains(@class,'product-title')]")
                    ?? parent.SelectSingleNode(".//h2")
                    ?? parent.SelectSingleNode(".//h3");
            }

            parent = parent.ParentNode;
        }

        return null;
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

        foreach (var separator in TitleSeparators)
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

        if (upper.Contains("TAPE") || upper.Contains("MC") || upper.Contains("CASSETT") || upper.Contains("CASSETE"))
        {
            return AlbumMediaType.Tape;
        }

        return null;
    }

    private static string? ExtractPriceFromOffer(JsonElement offer)
    {
        if (offer.TryGetProperty("price", out var priceElement))
        {
            return priceElement.ValueKind == JsonValueKind.Number
                ? priceElement.GetDecimal().ToString(System.Globalization.CultureInfo.InvariantCulture)
                : priceElement.GetString();
        }

        if (offer.TryGetProperty("priceSpecification", out var priceSpecElement))
        {
            var spec = priceSpecElement.ValueKind == JsonValueKind.Array && priceSpecElement.GetArrayLength() > 0
                ? priceSpecElement[0]
                : priceSpecElement;

            if (spec.TryGetProperty("price", out var specPrice))
            {
                return specPrice.ValueKind == JsonValueKind.Number
                    ? specPrice.GetDecimal().ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : specPrice.GetString();
            }
        }

        return null;
    }

    private static string? ExtractBrandName(JsonElement brandElement)
    {
        if (brandElement.ValueKind == JsonValueKind.Array && brandElement.GetArrayLength() > 0)
        {
            var firstBrand = brandElement[0];
            if (firstBrand.TryGetProperty("name", out var nameElement))
            {
                return nameElement.GetString();
            }
        }
        else if (brandElement.ValueKind == JsonValueKind.Object &&
                 brandElement.TryGetProperty("name", out var nameEl))
        {
            return nameEl.GetString();
        }

        return null;
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html))
        {
            return html;
        }

        var document = new HtmlDocument();
        document.LoadHtml(html);
        var text = document.DocumentNode.InnerText ?? string.Empty;
        text = HtmlEntity.DeEntitize(text);
        text = Regex.Replace(text, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", string.Empty);

        return text.Trim();
    }
}
