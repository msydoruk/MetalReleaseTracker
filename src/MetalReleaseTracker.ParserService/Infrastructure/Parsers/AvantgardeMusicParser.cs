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

public class AvantgardeMusicParser : BaseDistributorParser
{
    private const string BaseUrl = "https://www.sound-cave.com";

    public AvantgardeMusicParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        ISettingsService settingsService,
        ILogger<AvantgardeMusicParser> logger)
        : base(htmlDocumentLoader, settingsService, logger)
    {
    }

    public override DistributorCode DistributorCode => DistributorCode.AvantgardeMusic;

    protected override string[] CatalogueUrls =>
    [
        "https://www.sound-cave.com/en/shop/cd",
        "https://www.sound-cave.com/en/shop/vinyl"
    ];

    protected override string ParserName => "AvantgardeMusic";

    protected override AlbumMediaType[] CategoryMediaTypes =>
        [AlbumMediaType.CD, AlbumMediaType.LP];

    protected override List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productLinks = htmlDocument.DocumentNode.SelectNodes(AvantgardeMusicSelectors.ProductNodes);
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

            if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                href = BaseUrl + href;
            }

            var (bandName, albumTitle) = ParseListingTitle(linkNode);
            if (string.IsNullOrEmpty(bandName) && string.IsNullOrEmpty(albumTitle))
            {
                continue;
            }

            var rawTitle = $"{bandName} - {albumTitle}";
            results.Add(new ListingItem(bandName, albumTitle, href, rawTitle, CurrentCategoryMediaType));
        }

        return results;
    }

    protected override async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);
        var jsonLd = ParserHelper.ExtractProductJsonLd(htmlDocument);

        var bandName = ParseBandName(jsonLd, htmlDocument);
        var albumName = ParseAlbumName(jsonLd, htmlDocument);
        var sku = AlbumParsingHelper.GenerateSkuFromUrl(detailUrl);
        var price = ParsePrice(jsonLd, htmlDocument);
        var photoUrl = ParsePhotoUrl(jsonLd, htmlDocument);
        var label = ParseLabel(htmlDocument);
        var description = ParseDescription(jsonLd);
        var status = ParseStatus(jsonLd);
        var stockStatus = ParseStockStatus(jsonLd);

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
            Status = status,
            StockStatus = stockStatus
        };
    }

    protected override HtmlNode? FindNextPageLink(HtmlDocument htmlDocument)
    {
        var activeItem = htmlDocument.DocumentNode.SelectSingleNode(
            "//ul[contains(@class,'pagination')]//li[contains(@class,'active')]");

        if (activeItem == null)
        {
            return null;
        }

        var nextSibling = activeItem.NextSibling;
        while (nextSibling != null && nextSibling.NodeType != HtmlNodeType.Element)
        {
            nextSibling = nextSibling.NextSibling;
        }

        if (nextSibling == null)
        {
            return null;
        }

        var link = nextSibling.SelectSingleNode(".//a");
        if (link == null)
        {
            return null;
        }

        var text = link.InnerText?.Trim();
        if (text == "\u00bb")
        {
            return null;
        }

        return link;
    }

    protected override string TransformNextPageUrl(string url)
    {
        if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return BaseUrl + url;
        }

        return url;
    }

    protected override Exception CreateParserException(string message, Exception? innerException = null)
    {
        return innerException != null
            ? new AvantgardeMusicParserException(message, innerException)
            : new AvantgardeMusicParserException(message);
    }

    protected override bool IsOwnException(Exception exception)
    {
        return exception is AvantgardeMusicParserException;
    }

    private static (string BandName, string AlbumTitle) ParseListingTitle(HtmlNode linkNode)
    {
        var smallNode = linkNode.SelectSingleNode(".//small");

        if (smallNode != null)
        {
            var albumTitle = HtmlEntity.DeEntitize(smallNode.InnerText?.Trim() ?? string.Empty);

            var fullText = HtmlEntity.DeEntitize(linkNode.InnerText?.Trim() ?? string.Empty);
            var bandName = fullText.Replace(smallNode.InnerText ?? string.Empty, string.Empty).Trim();

            return (bandName, albumTitle);
        }

        var text = HtmlEntity.DeEntitize(linkNode.InnerText?.Trim() ?? string.Empty);
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line))
            .ToArray();

        if (lines.Length >= 2)
        {
            return (lines[0], lines[1]);
        }

        return (text, text);
    }

    private string ParseBandName(JsonElement? jsonLd, HtmlDocument htmlDocument)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("brand", out var brandElement))
        {
            if (brandElement.ValueKind == JsonValueKind.Object &&
                brandElement.TryGetProperty("name", out var nameElement))
            {
                var name = nameElement.GetString();
                if (!string.IsNullOrEmpty(name))
                {
                    return name;
                }
            }
        }

        var titleNode = htmlDocument.DocumentNode.SelectSingleNode(AvantgardeMusicSelectors.DetailTitle);
        if (titleNode != null)
        {
            var smallNode = titleNode.SelectSingleNode(".//small");
            if (smallNode != null)
            {
                var fullText = HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty);
                var bandName = fullText.Replace(smallNode.InnerText ?? string.Empty, string.Empty).Trim();
                if (!string.IsNullOrEmpty(bandName))
                {
                    return bandName;
                }
            }
        }

        return string.Empty;
    }

    private string ParseAlbumName(JsonElement? jsonLd, HtmlDocument htmlDocument)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("name", out var nameElement))
        {
            var name = nameElement.GetString();
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
        }

        var smallNode = htmlDocument.DocumentNode.SelectSingleNode(AvantgardeMusicSelectors.DetailTitleAlbum);
        if (smallNode != null)
        {
            var text = HtmlEntity.DeEntitize(smallNode.InnerText?.Trim() ?? string.Empty);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        return string.Empty;
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

        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(AvantgardeMusicSelectors.DetailPrice);
        var priceText = priceNode?.InnerText?.Trim();
        if (!string.IsNullOrEmpty(priceText))
        {
            priceText = HtmlEntity.DeEntitize(priceText);
            var match = Regex.Match(priceText, @"[\d]+[.,][\d]+");
            if (match.Success)
            {
                return AlbumParsingHelper.ParsePrice(match.Value.Replace(',', '.'));
            }

            var intMatch = Regex.Match(priceText, @"\d+");
            if (intMatch.Success)
            {
                return AlbumParsingHelper.ParsePrice(intMatch.Value);
            }
        }

        return 0.0f;
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

        var imgNode = htmlDocument.DocumentNode.SelectSingleNode(AvantgardeMusicSelectors.DetailPhoto);
        if (imgNode != null)
        {
            var src = imgNode.GetAttributeValue("src", null);
            if (!string.IsNullOrEmpty(src))
            {
                return src.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? src : BaseUrl + src;
            }
        }

        return string.Empty;
    }

    private string ParseLabel(HtmlDocument htmlDocument)
    {
        var specsNode = htmlDocument.DocumentNode.SelectSingleNode(AvantgardeMusicSelectors.DetailSpecs);
        if (specsNode != null)
        {
            var text = HtmlEntity.DeEntitize(specsNode.InnerText ?? string.Empty);
            var match = Regex.Match(text, @"label:\s*(.+?)(?:\n|$)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        var allParagraphs = htmlDocument.DocumentNode.SelectNodes("//p");
        if (allParagraphs != null)
        {
            foreach (var paragraph in allParagraphs)
            {
                var text = HtmlEntity.DeEntitize(paragraph.InnerText ?? string.Empty);
                var match = Regex.Match(text, @"label:\s*(.+?)(?:\n|$)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups[1].Value.Trim();
                }
            }
        }

        return string.Empty;
    }

    private string ParseDescription(JsonElement? jsonLd)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("description", out var descElement))
        {
            var description = descElement.GetString();
            if (!string.IsNullOrEmpty(description))
            {
                return ParserHelper.StripHtml(description);
            }
        }

        return string.Empty;
    }

    private AlbumStatus? ParseStatus(JsonElement? jsonLd)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("offers", out var offersElement))
        {
            var offer = offersElement.ValueKind == JsonValueKind.Array && offersElement.GetArrayLength() > 0
                ? offersElement[0]
                : offersElement;

            if (offer.TryGetProperty("availability", out var availElement))
            {
                var availability = availElement.GetString();
                if (availability != null && availability.Contains("PreOrder", StringComparison.OrdinalIgnoreCase))
                {
                    return AlbumStatus.PreOrder;
                }
            }
        }

        return null;
    }

    private static StockStatus ParseStockStatus(JsonElement? jsonLd)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("offers", out var offersElement))
        {
            return AlbumParsingHelper.ParseStockStatusFromJsonLd(offersElement);
        }

        return StockStatus.Unknown;
    }
}
