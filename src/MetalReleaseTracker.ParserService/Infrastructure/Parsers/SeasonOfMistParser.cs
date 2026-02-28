using System.Globalization;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers;

public class SeasonOfMistParser : BaseDistributorParser
{
    public SeasonOfMistParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        ISettingsService settingsService,
        ILogger<SeasonOfMistParser> logger)
        : base(htmlDocumentLoader, settingsService, logger)
    {
    }

    public override DistributorCode DistributorCode => DistributorCode.SeasonOfMist;

    protected override string[] CatalogueUrls =>
    [
        "https://shop.season-of-mist.com/music?cat=3",
        "https://shop.season-of-mist.com/music?cat=5",
        "https://shop.season-of-mist.com/music?cat=23"
    ];

    protected override string ParserName => "SeasonOfMist";

    protected override List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productItems = htmlDocument.DocumentNode.SelectNodes(SeasonOfMistSelectors.ProductGrid);
        if (productItems == null)
        {
            return results;
        }

        foreach (var item in productItems)
        {
            var nameLink = item.SelectSingleNode(SeasonOfMistSelectors.ProductNameLink);
            if (nameLink == null)
            {
                continue;
            }

            var href = HtmlEntity.DeEntitize(nameLink.GetAttributeValue("href", string.Empty).Trim());
            if (string.IsNullOrEmpty(href) || !processedUrls.Add(href))
            {
                continue;
            }

            var rawTitle = HtmlEntity.DeEntitize(nameLink.InnerText?.Trim() ?? string.Empty);
            if (string.IsNullOrEmpty(rawTitle))
            {
                continue;
            }

            var (bandName, albumTitle) = ParseProductName(rawTitle);

            results.Add(new ListingItem(bandName, albumTitle, href, rawTitle, CurrentCategoryMediaType));
        }

        return results;
    }

    protected override async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);

        var bandName = ParseAttributeValue(htmlDocument, "Band");
        var albumName = ParseAttributeValue(htmlDocument, "Title");
        var sku = ParseAttributeValue(htmlDocument, "Catalog #");
        var price = ParsePrice(htmlDocument);
        var photoUrl = ParsePhotoUrl(htmlDocument);
        var genre = ParseGenre(htmlDocument);
        var releaseDate = ParseReleaseDate(htmlDocument);
        var label = ParseLabel(htmlDocument);
        var status = ParseStatus(htmlDocument);

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
            Description = string.Empty,
            Status = status
        };
    }

    protected override HtmlNode? FindNextPageLink(HtmlDocument htmlDocument)
    {
        return htmlDocument.DocumentNode.SelectSingleNode(SeasonOfMistSelectors.NextPageLink);
    }

    protected override Exception CreateParserException(string message, Exception? innerException = null)
    {
        return innerException != null
            ? new SeasonOfMistParserException(message, innerException)
            : new SeasonOfMistParserException(message);
    }

    protected override bool IsOwnException(Exception exception)
    {
        return exception is SeasonOfMistParserException;
    }

    private string ParseAttributeValue(HtmlDocument htmlDocument, string attributeName)
    {
        var xpath = string.Format(SeasonOfMistSelectors.DetailAttributeHeader, attributeName);
        var thNode = htmlDocument.DocumentNode.SelectSingleNode(xpath);

        if (thNode != null)
        {
            var tdNode = thNode.SelectSingleNode("following-sibling::td");
            if (tdNode != null)
            {
                var text = HtmlEntity.DeEntitize(tdNode.InnerText?.Trim() ?? string.Empty);
                if (!string.IsNullOrEmpty(text))
                {
                    return text;
                }
            }
        }

        return string.Empty;
    }

    private float ParsePrice(HtmlDocument htmlDocument)
    {
        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(SeasonOfMistSelectors.DetailPrice);
        if (priceNode != null)
        {
            var priceText = HtmlEntity.DeEntitize(priceNode.InnerText?.Trim() ?? string.Empty)
                .Replace("€", string.Empty)
                .Replace(",", ".")
                .Trim();

            return AlbumParsingHelper.ParsePrice(priceText);
        }

        return 0.0f;
    }

    private string ParsePhotoUrl(HtmlDocument htmlDocument)
    {
        var imgNode = htmlDocument.DocumentNode.SelectSingleNode(SeasonOfMistSelectors.DetailPhoto)
            ?? htmlDocument.DocumentNode.SelectSingleNode(SeasonOfMistSelectors.DetailPhotoFallback);

        if (imgNode != null)
        {
            var src = imgNode.GetAttributeValue("src", string.Empty);
            if (!string.IsNullOrEmpty(src))
            {
                return src;
            }
        }

        return string.Empty;
    }

    private string ParseGenre(HtmlDocument htmlDocument)
    {
        var genre = ParseAttributeValue(htmlDocument, "Detailed musical style");
        if (!string.IsNullOrEmpty(genre))
        {
            return genre;
        }

        return ParseAttributeValue(htmlDocument, "Generic musical style");
    }

    private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
    {
        var dateText = ParseAttributeValue(htmlDocument, "Release Date");
        if (string.IsNullOrEmpty(dateText))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParseExact(dateText, "d MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTime.TryParseExact(dateText, "d MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        if (DateTime.TryParseExact(dateText, "dd MMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        return DateTime.MinValue;
    }

    private string ParseLabel(HtmlDocument htmlDocument)
    {
        return ParseAttributeValue(htmlDocument, "Label");
    }

    private AlbumStatus? ParseStatus(HtmlDocument htmlDocument)
    {
        var buttonNode = htmlDocument.DocumentNode.SelectSingleNode(SeasonOfMistSelectors.DetailCartButton)
            ?? htmlDocument.DocumentNode.SelectSingleNode(SeasonOfMistSelectors.DetailCartButtonFallback);

        if (buttonNode != null)
        {
            var buttonText = HtmlEntity.DeEntitize(buttonNode.InnerText?.Trim() ?? string.Empty);
            if (buttonText.Contains("Pre-Order", StringComparison.OrdinalIgnoreCase) ||
                buttonText.Contains("Preorder", StringComparison.OrdinalIgnoreCase))
            {
                return AlbumStatus.PreOrder;
            }
        }

        return null;
    }

    private static (string BandName, string AlbumTitle) ParseProductName(string rawTitle)
    {
        var parts = rawTitle.Split(" - ");
        if (parts.Length >= 3)
        {
            var bandName = parts[0].Trim();
            var albumTitle = string.Join(" - ", parts[1..^1]).Trim();
            return (bandName, albumTitle);
        }

        if (parts.Length == 2)
        {
            return (parts[0].Trim(), parts[1].Trim());
        }

        return (string.Empty, rawTitle.Trim());
    }
}
