using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers;

public class BlackMetalStoreParser : BaseDistributorParser
{
    private static readonly char[] TitleSeparators = ['\u2013', '\u2014', '-'];

    public BlackMetalStoreParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<BlackMetalStoreParser> logger)
        : base(htmlDocumentLoader, generalParserSettings, logger)
    {
    }

    public override DistributorCode DistributorCode => DistributorCode.BlackMetalStore;

    protected override string[] CatalogueUrls =>
    [
        "https://blackmetalstore.com/categoria-produto/cds/",
        "https://blackmetalstore.com/categoria-produto/vinyl/",
        "https://blackmetalstore.com/categoria-produto/cassettes/"
    ];

    protected override string ParserName => "BlackMetalStore";

    protected override List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productLinks = htmlDocument.DocumentNode.SelectNodes(BlackMetalStoreSelectors.ProductLinks);
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
            results.Add(new ListingItem(bandName, albumTitle, href, titleText, CurrentCategoryMediaType));
        }

        return results;
    }

    protected override async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);
        var jsonLd = ParserHelper.ExtractProductJsonLd(htmlDocument);

        var titleNode = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalStoreSelectors.DetailTitle);
        var titleText = titleNode != null
            ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
            : string.Empty;

        var (bandName, albumName, _) = SplitTitle(titleText);
        var sku = ParseSku(jsonLd, htmlDocument, detailUrl);
        var price = ParsePrice(jsonLd, htmlDocument);
        var photoUrl = ParsePhotoUrl(jsonLd, htmlDocument);
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
            Label = label,
            Press = sku,
            Description = description,
            Status = null
        };
    }

    protected override HtmlNode? FindNextPageLink(HtmlDocument htmlDocument)
    {
        return htmlDocument.DocumentNode.SelectSingleNode(BlackMetalStoreSelectors.NextPageLink);
    }

    protected override Exception CreateParserException(string message, Exception? innerException = null)
    {
        return innerException != null
            ? new BlackMetalStoreParserException(message, innerException)
            : new BlackMetalStoreParserException(message);
    }

    protected override bool IsOwnException(Exception exception)
    {
        return exception is BlackMetalStoreParserException;
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

        var skuNode = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalStoreSelectors.DetailSku);
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

        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalStoreSelectors.DetailPrice)
            ?? htmlDocument.DocumentNode.SelectSingleNode(BlackMetalStoreSelectors.DetailPriceFallback);

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

        var imgNode = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalStoreSelectors.DetailPhoto);
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
        var categoryLinks = htmlDocument.DocumentNode.SelectNodes(BlackMetalStoreSelectors.DetailLabel);
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

        var brandNode = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalStoreSelectors.DetailBrand);
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
                return ParserHelper.StripHtml(description);
            }
        }

        var descNode = htmlDocument.DocumentNode.SelectSingleNode(BlackMetalStoreSelectors.DetailDescription);

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

    private static HtmlNode? FindTitleForProduct(HtmlNode linkNode)
    {
        var parent = linkNode.ParentNode;
        while (parent != null)
        {
            if (parent.HasClass("product"))
            {
                return parent.SelectSingleNode(BlackMetalStoreSelectors.ProductTitle)
                    ?? parent.SelectSingleNode(BlackMetalStoreSelectors.ProductTitleFallback)
                    ?? parent.SelectSingleNode(BlackMetalStoreSelectors.ProductTitleFallback2);
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
}
