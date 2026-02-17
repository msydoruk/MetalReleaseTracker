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

public class ParagonRecordsParser : BaseDistributorParser
{
    private const string BaseUrl = "https://www.paragonrecords.org";

    private static readonly string[] FormatTokens =
    [
        "CASSETTE", "DIGIPAK", "DIGISLEEVE", "GATEFOLD", "DOUBLE", "DIGI",
        "COLOURED", "COLORED", "DLP", "LP", "DCD", "CD", "EP", "TAPE"
    ];

    public ParagonRecordsParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<ParagonRecordsParser> logger)
        : base(htmlDocumentLoader, generalParserSettings, logger)
    {
    }

    public override DistributorCode DistributorCode => DistributorCode.ParagonRecords;

    protected override string[] CatalogueUrls =>
    [
        "https://www.paragonrecords.org/collections/cd",
        "https://www.paragonrecords.org/collections/vinyl",
        "https://www.paragonrecords.org/collections/cassette"
    ];

    protected override string ParserName => "ParagonRecords";

    protected override List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productGrid = htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.ProductGrid);
        if (productGrid == null)
        {
            return results;
        }

        var productItems = productGrid.SelectNodes(ParagonRecordsSelectors.ProductItems);
        if (productItems == null)
        {
            return results;
        }

        foreach (var item in productItems)
        {
            var linkNode = item.SelectSingleNode(ParagonRecordsSelectors.ProductLink);
            if (linkNode == null)
            {
                continue;
            }

            var href = HtmlEntity.DeEntitize(linkNode.GetAttributeValue("href", string.Empty).Trim());
            if (string.IsNullOrEmpty(href))
            {
                continue;
            }

            var absoluteUrl = ToAbsoluteUrl(href);
            if (!processedUrls.Add(absoluteUrl))
            {
                continue;
            }

            var titleNode = item.SelectSingleNode(ParagonRecordsSelectors.ProductTitle);
            var rawTitle = titleNode != null
                ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
                : string.Empty;

            if (string.IsNullOrEmpty(rawTitle))
            {
                continue;
            }

            var (bandName, albumTitle) = ParseProductName(rawTitle);

            results.Add(new ListingItem(bandName, albumTitle, absoluteUrl, rawTitle, CurrentCategoryMediaType));
        }

        return results;
    }

    protected override async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);
        var pageSource = htmlDocument.DocumentNode.OuterHtml;

        var titleText = ParseTitle(htmlDocument);
        var (bandName, albumName) = ParseProductName(titleText);
        var price = ParsePrice(htmlDocument);
        var photoUrl = ParsePhotoUrl(htmlDocument);
        var genre = ParseGenre(htmlDocument);
        var label = ParseLabel(pageSource);
        var status = ParseStatus(htmlDocument);

        return new AlbumParsedEvent
        {
            DistributorCode = DistributorCode,
            BandName = bandName,
            SKU = AlbumParsingHelper.GenerateSkuFromUrl(detailUrl),
            Name = albumName,
            ReleaseDate = DateTime.MinValue,
            Genre = genre,
            Price = price,
            PurchaseUrl = detailUrl,
            PhotoUrl = photoUrl,
            Label = label,
            Press = string.Empty,
            Description = string.Empty,
            Status = status
        };
    }

    protected override HtmlNode? FindNextPageLink(HtmlDocument htmlDocument)
    {
        return htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.NextPageLink);
    }

    protected override string TransformNextPageUrl(string url)
    {
        return ToAbsoluteUrl(url);
    }

    protected override Exception CreateParserException(string message, Exception? innerException = null)
    {
        return innerException != null
            ? new ParagonRecordsParserException(message, innerException)
            : new ParagonRecordsParserException(message);
    }

    protected override bool IsOwnException(Exception exception)
    {
        return exception is ParagonRecordsParserException;
    }

    private string ParseTitle(HtmlDocument htmlDocument)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.DetailTitle)
            ?? htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.DetailTitleFallback);

        return titleNode != null
            ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
            : string.Empty;
    }

    private float ParsePrice(HtmlDocument htmlDocument)
    {
        var ogPrice = htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.DetailOgPrice);
        if (ogPrice != null)
        {
            var priceText = ogPrice.GetAttributeValue("content", string.Empty);
            if (!string.IsNullOrEmpty(priceText))
            {
                return AlbumParsingHelper.ParsePrice(priceText);
            }
        }

        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.DetailPrice);
        if (priceNode != null)
        {
            var priceText = HtmlEntity.DeEntitize(priceNode.InnerText?.Trim() ?? string.Empty)
                .Replace("$", string.Empty)
                .Trim();

            return AlbumParsingHelper.ParsePrice(priceText);
        }

        return 0.0f;
    }

    private string ParsePhotoUrl(HtmlDocument htmlDocument)
    {
        var ogImage = htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.DetailOgImageSecure);
        if (ogImage != null)
        {
            var content = ogImage.GetAttributeValue("content", string.Empty);
            if (!string.IsNullOrEmpty(content))
            {
                return content;
            }
        }

        ogImage = htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.DetailOgImage);
        if (ogImage != null)
        {
            var content = ogImage.GetAttributeValue("content", string.Empty);
            if (!string.IsNullOrEmpty(content))
            {
                return content.Replace("http://", "https://");
            }
        }

        return string.Empty;
    }

    private string ParseGenre(HtmlDocument htmlDocument)
    {
        var descNode = htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.DetailDescription);

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

    private AlbumStatus? ParseStatus(HtmlDocument htmlDocument)
    {
        var buttonSpan = htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.DetailCartButton)
            ?? htmlDocument.DocumentNode.SelectSingleNode(ParagonRecordsSelectors.DetailCartButtonFallback);

        if (buttonSpan != null)
        {
            var buttonText = HtmlEntity.DeEntitize(buttonSpan.InnerText?.Trim() ?? string.Empty);
            if (buttonText.Contains("Pre-Order", StringComparison.OrdinalIgnoreCase) ||
                buttonText.Contains("Preorder", StringComparison.OrdinalIgnoreCase))
            {
                return AlbumStatus.PreOrder;
            }
        }

        return null;
    }

    private static string ToAbsoluteUrl(string href)
    {
        if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return href;
        }

        return BaseUrl + href;
    }

    private static (string BandName, string AlbumTitle) ParseProductName(string rawTitle)
    {
        var parts = rawTitle.Split(" - ", 2);
        if (parts.Length == 2)
        {
            var bandName = parts[0].Trim();
            var albumWithFormat = parts[1].Trim();
            var albumTitle = StripFormatSuffix(albumWithFormat);
            return (bandName, albumTitle);
        }

        return (string.Empty, rawTitle.Trim());
    }

    private static string StripFormatSuffix(string albumText)
    {
        var result = albumText.TrimEnd();
        bool changed;

        do
        {
            changed = false;
            foreach (var token in FormatTokens)
            {
                if (result.EndsWith($" {token}", StringComparison.OrdinalIgnoreCase))
                {
                    result = result[..^(token.Length + 1)].TrimEnd();
                    changed = true;
                    break;
                }
            }
        }
        while (changed && result.Length > 0);

        return result.Length > 0 ? result : albumText.TrimEnd();
    }

    private static string ParseLabel(string pageSource)
    {
        var match = Regex.Match(
            pageSource,
            @"""product"":\{[^}]*""vendor"":""([^""]+)""");

        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}
