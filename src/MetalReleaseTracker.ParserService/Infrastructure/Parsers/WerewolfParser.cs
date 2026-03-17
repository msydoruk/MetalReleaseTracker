using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers;

public class WerewolfParser : BaseDistributorParser
{
    private static readonly char[] TitleSeparators = ['\u2013', '\u2014', '-'];

    public WerewolfParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        ISettingsService settingsService,
        ILogger<WerewolfParser> logger)
        : base(htmlDocumentLoader, settingsService, logger)
    {
    }

    public override DistributorCode DistributorCode => DistributorCode.Werewolf;

    protected override string[] CatalogueUrls =>
    [
        "https://werewolf-webshop.pl/product-category/cds?v=3943d8795e03",
        "https://werewolf-webshop.pl/product-category/vinyls-2?v=3943d8795e03"
    ];

    protected override string ParserName => "Werewolf";

    protected override AlbumMediaType[] CategoryMediaTypes =>
        [AlbumMediaType.CD, AlbumMediaType.LP];

    protected override List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productNodes = htmlDocument.DocumentNode.SelectNodes(WerewolfSelectors.ProductNodes);
        if (productNodes == null)
        {
            return results;
        }

        foreach (var productNode in productNodes)
        {
            var anchorNode = productNode.SelectSingleNode(WerewolfSelectors.ProductAnchor);
            if (anchorNode == null)
            {
                continue;
            }

            var href = HtmlEntity.DeEntitize(anchorNode.GetAttributeValue("href", string.Empty).Trim());
            if (string.IsNullOrEmpty(href) || !processedUrls.Add(href))
            {
                continue;
            }

            var titleText = HtmlEntity.DeEntitize(anchorNode.InnerText?.Trim() ?? string.Empty);
            if (string.IsNullOrEmpty(titleText))
            {
                continue;
            }

            var (bandName, albumTitle) = SplitTitle(titleText);
            results.Add(new ListingItem(bandName, albumTitle, href, titleText, CurrentCategoryMediaType));
        }

        return results;
    }

    protected override async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);
        var jsonLd = ParserHelper.ExtractProductJsonLd(htmlDocument);

        var (bandName, albumName) = ParseTitle(htmlDocument);
        var sku = ParseSku(jsonLd, detailUrl);
        var price = ParsePrice(jsonLd, htmlDocument);
        var photoUrl = ParsePhotoUrl(jsonLd, htmlDocument);
        var label = ParseLabel(htmlDocument);
        var description = ParseDescription(jsonLd, htmlDocument);

        return new AlbumParsedEvent
        {
            DistributorCode = DistributorCode,
            BandName = bandName,
            SKU = sku,
            Name = albumName,
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
        return htmlDocument.DocumentNode.SelectSingleNode(WerewolfSelectors.NextPageLink);
    }

    protected override Exception CreateParserException(string message, Exception? innerException = null)
    {
        return innerException != null
            ? new WerewolfParserException(message, innerException)
            : new WerewolfParserException(message);
    }

    protected override bool IsOwnException(Exception exception)
    {
        return exception is WerewolfParserException;
    }

    private (string BandName, string AlbumName) ParseTitle(HtmlDocument htmlDocument)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode(WerewolfSelectors.DetailTitle);
        var titleText = titleNode?.InnerText?.Trim();

        if (string.IsNullOrEmpty(titleText))
        {
            return (string.Empty, string.Empty);
        }

        titleText = HtmlEntity.DeEntitize(titleText);
        return SplitTitle(titleText);
    }

    private float ParsePrice(JsonElement? jsonLd, HtmlDocument htmlDocument)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("offers", out var offersElement))
        {
            var offer = offersElement.ValueKind == JsonValueKind.Array && offersElement.GetArrayLength() > 0
                ? offersElement[0]
                : offersElement;

            if (offer.TryGetProperty("price", out var priceElement))
            {
                var priceStr = priceElement.ValueKind == JsonValueKind.Number
                    ? priceElement.GetDecimal().ToString(System.Globalization.CultureInfo.InvariantCulture)
                    : priceElement.GetString();

                if (!string.IsNullOrEmpty(priceStr))
                {
                    return AlbumParsingHelper.ParsePrice(priceStr);
                }
            }
        }

        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(WerewolfSelectors.DetailPrice)
            ?? htmlDocument.DocumentNode.SelectSingleNode(WerewolfSelectors.DetailPriceFallback);

        var priceText = priceNode?.InnerText?.Trim();
        if (string.IsNullOrEmpty(priceText))
        {
            return 0.0f;
        }

        priceText = HtmlEntity.DeEntitize(priceText);
        var match = Regex.Match(priceText, @"[\d]+[.,][\d]+");

        return match.Success ? AlbumParsingHelper.ParsePrice(match.Value.Replace(',', '.')) : 0.0f;
    }

    private string ParseSku(JsonElement? jsonLd, string detailUrl)
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

        return AlbumParsingHelper.GenerateSkuFromUrl(detailUrl);
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

        var galleryLink = htmlDocument.DocumentNode.SelectSingleNode(
            "//div[contains(@class,'woocommerce-product-gallery')]//a[@href]");
        if (galleryLink != null)
        {
            var href = galleryLink.GetAttributeValue("href", string.Empty).Trim();
            if (!string.IsNullOrEmpty(href) && !href.StartsWith("#"))
            {
                return href;
            }
        }

        var imgNode = htmlDocument.DocumentNode.SelectSingleNode(WerewolfSelectors.DetailPhotoFallback);
        if (imgNode != null)
        {
            var src = imgNode.GetAttributeValue("data-src", null)
                ?? imgNode.GetAttributeValue("data-large_image", null)
                ?? imgNode.GetAttributeValue("src", null);

            if (!string.IsNullOrEmpty(src) && !src.StartsWith("data:"))
            {
                return Regex.Replace(src, @"-\d+x\d+(?=\.\w+$)", string.Empty);
            }
        }

        return string.Empty;
    }

    private string ParseLabel(HtmlDocument htmlDocument)
    {
        var brandNode = htmlDocument.DocumentNode.SelectSingleNode(WerewolfSelectors.DetailBrand);
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

        var descNode = htmlDocument.DocumentNode.SelectSingleNode(WerewolfSelectors.DetailShortDescription);
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

    private static (string BandName, string AlbumTitle) SplitTitle(string title)
    {
        if (string.IsNullOrEmpty(title))
        {
            return (string.Empty, string.Empty);
        }

        foreach (var separator in TitleSeparators)
        {
            var separatorWithSpaces = $" {separator} ";
            var parts = title.Split(new[] { separatorWithSpaces }, 2, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                return (parts[0].Trim(), parts[1].Trim());
            }
        }

        return (title, title);
    }
}
