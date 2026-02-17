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

public class DrakkarParser : BaseDistributorParser
{
    private static readonly char[] TitleSeparators = ['\u2013', '\u2014', '-'];

    public DrakkarParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<DrakkarParser> logger)
        : base(htmlDocumentLoader, generalParserSettings, logger)
    {
    }

    public override DistributorCode DistributorCode => DistributorCode.Drakkar;

    protected override string[] CatalogueUrls =>
    [
        "https://www.drakkar666.com/product-category/audio/cds/",
        "https://www.drakkar666.com/product-category/audio/vinyls-7ep/",
        "https://www.drakkar666.com/product-category/audio/vinyls-1210lp/",
        "https://www.drakkar666.com/product-category/audio/tapes/"
    ];

    protected override string ParserName => "Drakkar";

    protected override AlbumMediaType[] CategoryMediaTypes =>
        [AlbumMediaType.CD, AlbumMediaType.LP, AlbumMediaType.LP, AlbumMediaType.Tape];

    protected override List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productNodes = htmlDocument.DocumentNode.SelectNodes(DrakkarSelectors.ProductNodes)
            ?? htmlDocument.DocumentNode.SelectNodes(DrakkarSelectors.ProductNodesFallback);

        if (productNodes == null)
        {
            return results;
        }

        foreach (var productNode in productNodes)
        {
            var anchorNode = productNode.SelectSingleNode(DrakkarSelectors.ProductAnchor);
            if (anchorNode == null)
            {
                continue;
            }

            var href = anchorNode.GetAttributeValue("href", string.Empty).Trim();
            if (string.IsNullOrEmpty(href) || !processedUrls.Add(href))
            {
                continue;
            }

            var titleNode = productNode.SelectSingleNode(DrakkarSelectors.ProductTitle);
            var titleText = titleNode != null
                ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
                : string.Empty;

            var (bandName, albumTitle, _) = ParseTitleParts(titleText);

            results.Add(new ListingItem(bandName, albumTitle, href, titleText, CurrentCategoryMediaType));
        }

        return results;
    }

    protected override async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);
        var jsonLd = ParserHelper.ExtractProductJsonLd(htmlDocument);

        var (bandName, albumName, _) = ParseTitle(htmlDocument);
        var sku = ParseSku(jsonLd, htmlDocument, detailUrl);
        var price = ParsePrice(htmlDocument);
        var photoUrl = ParsePhotoUrl(jsonLd, htmlDocument);
        var label = ParseLabel(jsonLd, htmlDocument);
        var genre = ParseGenre(htmlDocument);
        var releaseDate = ParseReleaseDate(htmlDocument);
        var description = ParseDescription(jsonLd, htmlDocument);

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
            Label = label,
            Press = sku,
            Description = description,
            Status = null
        };
    }

    protected override HtmlNode? FindNextPageLink(HtmlDocument htmlDocument)
    {
        return htmlDocument.DocumentNode.SelectSingleNode(DrakkarSelectors.NextPageLink);
    }

    protected override Exception CreateParserException(string message, Exception? innerException = null)
    {
        return innerException != null
            ? new DrakkarParserException(message, innerException)
            : new DrakkarParserException(message);
    }

    protected override bool IsOwnException(Exception exception)
    {
        return exception is DrakkarParserException;
    }

    private (string BandName, string AlbumName, string MediaTypeRaw) ParseTitle(HtmlDocument htmlDocument)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode(DrakkarSelectors.DetailTitle);
        var titleText = titleNode?.InnerText?.Trim();

        if (string.IsNullOrEmpty(titleText))
        {
            return (string.Empty, string.Empty, string.Empty);
        }

        titleText = HtmlEntity.DeEntitize(titleText);
        return ParseTitleParts(titleText);
    }

    private float ParsePrice(HtmlDocument htmlDocument)
    {
        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(DrakkarSelectors.DetailPrice)
            ?? htmlDocument.DocumentNode.SelectSingleNode(DrakkarSelectors.DetailPriceFallback);

        var priceText = priceNode?.InnerText?.Trim();
        if (string.IsNullOrEmpty(priceText))
        {
            return 0.0f;
        }

        priceText = HtmlEntity.DeEntitize(priceText);
        var match = Regex.Match(priceText, @"[\d]+[.,][\d]+");

        return match.Success ? AlbumParsingHelper.ParsePrice(match.Value.Replace(',', '.')) : 0.0f;
    }

    private string ParseSku(JsonElement? jsonLd, HtmlDocument htmlDocument, string albumUrl)
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

        var skuNode = htmlDocument.DocumentNode.SelectSingleNode(DrakkarSelectors.DetailSku);
        var sku = skuNode?.InnerText?.Trim();

        if (!string.IsNullOrEmpty(sku))
        {
            return sku;
        }

        var slugMatch = Regex.Match(albumUrl, @"/product/([^/]+)");
        return slugMatch.Success ? slugMatch.Groups[1].Value : albumUrl;
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

        var imgNode = htmlDocument.DocumentNode.SelectSingleNode(DrakkarSelectors.DetailPhotoGallery);

        if (imgNode != null)
        {
            var dataSrc = imgNode.GetAttributeValue("data-src", null)
                ?? imgNode.GetAttributeValue("data-large_image", null)
                ?? imgNode.GetAttributeValue("src", null);

            if (!string.IsNullOrEmpty(dataSrc) && !dataSrc.StartsWith("data:"))
            {
                return Regex.Replace(dataSrc, @"-\d+x\d+(?=\.\w+$)", string.Empty);
            }
        }

        return string.Empty;
    }

    private string ParseGenre(HtmlDocument htmlDocument)
    {
        var lines = GetStructuredTextLines(htmlDocument, DrakkarSelectors.DetailShortDescription);

        foreach (var line in lines)
        {
            if (!string.IsNullOrWhiteSpace(line)
                && !line.Contains("Origin", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("Label", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("Release Year", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("Weight", StringComparison.OrdinalIgnoreCase))
            {
                return line.Trim();
            }
        }

        var attrLines = GetStructuredTextLines(htmlDocument, DrakkarSelectors.DetailAttributes);

        foreach (var line in attrLines)
        {
            if (!string.IsNullOrWhiteSpace(line)
                && !line.Contains("Origin", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("Label", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("Release Year", StringComparison.OrdinalIgnoreCase)
                && !line.Contains("Weight", StringComparison.OrdinalIgnoreCase))
            {
                return line.Trim();
            }
        }

        return string.Empty;
    }

    private string ParseLabel(JsonElement? jsonLd, HtmlDocument htmlDocument)
    {
        if (jsonLd.HasValue && jsonLd.Value.TryGetProperty("brand", out var brandElement))
        {
            if (brandElement.ValueKind == JsonValueKind.Array && brandElement.GetArrayLength() > 0)
            {
                var firstBrand = brandElement[0];
                if (firstBrand.TryGetProperty("name", out var nameElement))
                {
                    var brandName = nameElement.GetString();
                    if (!string.IsNullOrEmpty(brandName))
                    {
                        return brandName;
                    }
                }
            }
            else if (brandElement.ValueKind == JsonValueKind.Object &&
                     brandElement.TryGetProperty("name", out var nameEl))
            {
                var brandName = nameEl.GetString();
                if (!string.IsNullOrEmpty(brandName))
                {
                    return brandName;
                }
            }
        }

        var lines = GetStructuredTextLines(htmlDocument, DrakkarSelectors.DetailShortDescription);

        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"Label\s*:\s*(.+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return string.Empty;
    }

    private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
    {
        var allLines = GetStructuredTextLines(htmlDocument, DrakkarSelectors.DetailShortDescription);
        allLines.AddRange(GetStructuredTextLines(htmlDocument, DrakkarSelectors.DetailAttributes));

        foreach (var line in allLines)
        {
            var match = Regex.Match(line, @"Release Year\s*:\s*(\d{4})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return AlbumParsingHelper.ParseYear(match.Groups[1].Value);
            }
        }

        var categoryLinks = htmlDocument.DocumentNode.SelectNodes(DrakkarSelectors.DetailCategoryLinks);
        if (categoryLinks != null)
        {
            foreach (var link in categoryLinks)
            {
                var text = link.InnerText?.Trim();
                if (!string.IsNullOrEmpty(text) && Regex.IsMatch(text, @"^\d{4}$"))
                {
                    return AlbumParsingHelper.ParseYear(text);
                }
            }
        }

        return DateTime.MinValue;
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

        var descNode = htmlDocument.DocumentNode.SelectSingleNode(DrakkarSelectors.DetailShortDescription);

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

    private List<string> GetStructuredTextLines(HtmlDocument htmlDocument, string xpath)
    {
        var node = htmlDocument.DocumentNode.SelectSingleNode(xpath);
        if (node == null)
        {
            return new List<string>();
        }

        var html = node.InnerHtml;
        html = Regex.Replace(
            html,
            @"<\s*/?\s*(p|div|br|li|tr|th|td|h[1-6])\b[^>]*>",
            "\n",
            RegexOptions.IgnoreCase);
        html = Regex.Replace(html, @"<[^>]+>", string.Empty);
        html = HtmlEntity.DeEntitize(html);

        return html.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .ToList();
    }

    private static string CapitalizeFirstLetter(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return char.ToUpper(text[0]) + text[1..];
    }

    private static (string BandName, string AlbumTitle, string MediaTypeRaw) ParseTitleParts(string titleText)
    {
        if (string.IsNullOrEmpty(titleText))
        {
            return (string.Empty, string.Empty, string.Empty);
        }

        string[] parts = null;
        foreach (var separator in TitleSeparators)
        {
            var separatorWithSpaces = $" {separator} ";
            var splitParts = titleText.Split(new[] { separatorWithSpaces }, StringSplitOptions.None);
            if (splitParts.Length >= 3)
            {
                parts = splitParts;
                break;
            }
        }

        if (parts == null || parts.Length < 3)
        {
            foreach (var separator in TitleSeparators)
            {
                var separatorWithSpaces = $" {separator} ";
                var splitParts = titleText.Split(new[] { separatorWithSpaces }, StringSplitOptions.None);
                if (splitParts.Length == 2)
                {
                    parts = splitParts;
                    break;
                }
            }
        }

        if (parts == null || parts.Length < 2)
        {
            return (titleText, titleText, string.Empty);
        }

        var bandName = parts[0].Trim();
        var mediaTypeRaw = parts[^1].Trim();

        if (parts.Length == 2)
        {
            return (bandName, CapitalizeFirstLetter(mediaTypeRaw), string.Empty);
        }

        var albumName = CapitalizeFirstLetter(string.Join(" - ", parts[1..^1]).Trim());
        return (bandName, albumName, mediaTypeRaw);
    }
}
