using System.Globalization;
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

public class NapalmRecordsParser : BaseDistributorParser
{
    public NapalmRecordsParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        ISettingsService settingsService,
        ILogger<NapalmRecordsParser> logger)
        : base(htmlDocumentLoader, settingsService, logger)
    {
    }

    public override DistributorCode DistributorCode => DistributorCode.NapalmRecords;

    protected override string[] CatalogueUrls =>
    [
        "https://napalmrecords.com/english/music/cds?product_list_dir=desc&product_list_order=release_date",
        "https://napalmrecords.com/english/music/lps?product_list_dir=desc&product_list_order=release_date",
        "https://napalmrecords.com/english/music/tapes?product_list_dir=desc&product_list_order=release_date"
    ];

    protected override string ParserName => "NapalmRecords";

    protected override List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var productItems = htmlDocument.DocumentNode.SelectNodes(NapalmRecordsSelectors.ProductItems);
        if (productItems == null)
        {
            return results;
        }

        foreach (var item in productItems)
        {
            var linkNode = item.SelectSingleNode(NapalmRecordsSelectors.ProductLink);
            if (linkNode == null)
            {
                continue;
            }

            var href = HtmlEntity.DeEntitize(linkNode.GetAttributeValue("href", string.Empty).Trim());
            if (string.IsNullOrEmpty(href) || !processedUrls.Add(href))
            {
                continue;
            }

            var albumTitle = HtmlEntity.DeEntitize(linkNode.InnerText?.Trim() ?? string.Empty);
            if (string.IsNullOrEmpty(albumTitle))
            {
                continue;
            }

            var bandNameNode = item.SelectSingleNode(NapalmRecordsSelectors.ProductBandName);
            var bandName = bandNameNode != null
                ? HtmlEntity.DeEntitize(bandNameNode.InnerText?.Trim() ?? string.Empty)
                : string.Empty;

            var rawTitle = !string.IsNullOrEmpty(bandName) ? $"{bandName} - {albumTitle}" : albumTitle;

            results.Add(new ListingItem(bandName, albumTitle, href, rawTitle, CurrentCategoryMediaType));
        }

        return results;
    }

    protected override async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);
        var pageSource = htmlDocument.DocumentNode.OuterHtml;

        var bandName = ParseAttribute(htmlDocument, "Band");
        var albumName = ParseAlbumName(htmlDocument);
        var sku = ParseSku(htmlDocument);
        var price = ParsePrice(pageSource, htmlDocument);
        var photoUrl = ParsePhotoUrl(htmlDocument);
        var genre = ParseAttribute(htmlDocument, "Genre");
        var releaseDate = ParseReleaseDate(htmlDocument);
        var description = ParseDescription(htmlDocument);

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
            Label = "Napalm Records",
            Press = sku,
            Description = description,
            Status = null
        };
    }

    protected override HtmlNode? FindNextPageLink(HtmlDocument htmlDocument)
    {
        return htmlDocument.DocumentNode.SelectSingleNode(NapalmRecordsSelectors.NextPageLink);
    }

    protected override Exception CreateParserException(string message, Exception? innerException = null)
    {
        return innerException != null
            ? new NapalmRecordsParserException(message, innerException)
            : new NapalmRecordsParserException(message);
    }

    protected override bool IsOwnException(Exception exception)
    {
        return exception is NapalmRecordsParserException;
    }

    private string ParseAlbumName(HtmlDocument htmlDocument)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode(NapalmRecordsSelectors.DetailTitle)
            ?? htmlDocument.DocumentNode.SelectSingleNode(NapalmRecordsSelectors.DetailTitleFallback);

        return titleNode != null
            ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
            : string.Empty;
    }

    private string ParseAttribute(HtmlDocument htmlDocument, string attributeName)
    {
        var xpath = string.Format(NapalmRecordsSelectors.DetailAttributeTable, attributeName);
        var cell = htmlDocument.DocumentNode.SelectSingleNode(xpath);

        if (cell != null)
        {
            var text = HtmlEntity.DeEntitize(cell.InnerText?.Trim() ?? string.Empty);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        return string.Empty;
    }

    private string ParseSku(HtmlDocument htmlDocument)
    {
        var strongNodes = htmlDocument.DocumentNode.SelectNodes(NapalmRecordsSelectors.DetailSkuStrong);
        if (strongNodes != null)
        {
            foreach (var strong in strongNodes)
            {
                var text = strong.InnerText?.Trim() ?? string.Empty;
                if (text.Contains("Art", StringComparison.OrdinalIgnoreCase) &&
                    text.Contains("Nr", StringComparison.OrdinalIgnoreCase))
                {
                    var parentText = strong.ParentNode?.InnerText?.Trim() ?? string.Empty;
                    var match = Regex.Match(parentText, @"(\d+)");
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
        }

        var formNode = htmlDocument.DocumentNode.SelectSingleNode(NapalmRecordsSelectors.DetailSkuForm);
        if (formNode != null)
        {
            var sku = formNode.GetAttributeValue("data-product-sku", string.Empty);
            if (!string.IsNullOrEmpty(sku))
            {
                return sku;
            }
        }

        return string.Empty;
    }

    private float ParsePrice(string pageSource, HtmlDocument htmlDocument)
    {
        var jsMatch = Regex.Match(pageSource, @"""final_price""[:\s]*""?([\d.]+)""?");
        if (jsMatch.Success)
        {
            return AlbumParsingHelper.ParsePrice(jsMatch.Groups[1].Value);
        }

        var priceNode = htmlDocument.DocumentNode.SelectSingleNode(NapalmRecordsSelectors.DetailPriceWrapper);
        if (priceNode != null)
        {
            var priceAmount = priceNode.GetAttributeValue("data-price-amount", string.Empty);
            if (!string.IsNullOrEmpty(priceAmount))
            {
                return AlbumParsingHelper.ParsePrice(priceAmount);
            }
        }

        return 0.0f;
    }

    private string ParsePhotoUrl(HtmlDocument htmlDocument)
    {
        var ogImage = htmlDocument.DocumentNode.SelectSingleNode(NapalmRecordsSelectors.DetailOgImage);
        if (ogImage != null)
        {
            var content = ogImage.GetAttributeValue("content", string.Empty);
            if (!string.IsNullOrEmpty(content))
            {
                return content;
            }
        }

        var galleryImg = htmlDocument.DocumentNode.SelectSingleNode(NapalmRecordsSelectors.DetailGalleryImage);
        if (galleryImg != null)
        {
            var src = galleryImg.GetAttributeValue("src", string.Empty);
            if (!string.IsNullOrEmpty(src))
            {
                return src;
            }
        }

        return string.Empty;
    }

    private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
    {
        var dateText = ParseAttribute(htmlDocument, "Release Date");
        if (string.IsNullOrEmpty(dateText))
        {
            return DateTime.MinValue;
        }

        if (DateTime.TryParseExact(dateText, "MMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            return date;
        }

        if (DateTime.TryParseExact(dateText, "MMMM d, yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            return date;
        }

        var yearMatch = Regex.Match(dateText, @"\d{4}");
        if (yearMatch.Success)
        {
            return AlbumParsingHelper.ParseYear(yearMatch.Value);
        }

        return DateTime.MinValue;
    }

    private string ParseDescription(HtmlDocument htmlDocument)
    {
        var descNode = htmlDocument.DocumentNode.SelectSingleNode(NapalmRecordsSelectors.DetailDescription);

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
}
