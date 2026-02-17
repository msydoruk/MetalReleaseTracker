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

public class OsmoseProductionsParser : BaseDistributorParser
{
    public OsmoseProductionsParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<OsmoseProductionsParser> logger)
        : base(htmlDocumentLoader, generalParserSettings, logger)
    {
    }

    public override DistributorCode DistributorCode => DistributorCode.OsmoseProductions;

    protected override string[] CatalogueUrls =>
    [
        "https://www.osmoseproductions.com/liste/?what=label&tete=osmose&srt=2&fmt=11",
        "https://www.osmoseproductions.com/liste/?what=label&tete=osmose&srt=2&fmt=990001",
        "https://www.osmoseproductions.com/liste/?what=label&tete=osmose&srt=2&fmt=16"
    ];

    protected override string ParserName => "OsmoseProductions";

    protected override List<ListingItem> ParseListingsFromPage(HtmlDocument htmlDocument)
    {
        var results = new List<ListingItem>();
        var albumNodes = htmlDocument.DocumentNode.SelectNodes(OsmoseProductionsSelectors.AlbumNodes);

        if (albumNodes == null)
        {
            return results;
        }

        foreach (var albumNode in albumNodes)
        {
            var anchorNode = albumNode.SelectSingleNode(OsmoseProductionsSelectors.AnchorNode);
            if (anchorNode == null)
            {
                continue;
            }

            var albumUrl = anchorNode.GetAttributeValue("href", string.Empty).Trim();
            if (string.IsNullOrEmpty(albumUrl))
            {
                continue;
            }

            var bandName = anchorNode.SelectSingleNode(OsmoseProductionsSelectors.BandNameSpan)?.InnerText?.Trim() ?? string.Empty;
            var albumTitle = anchorNode.SelectSingleNode(OsmoseProductionsSelectors.AlbumTitleSpan)?.InnerText?.Trim() ?? string.Empty;
            var rawTitle = string.IsNullOrEmpty(albumTitle) ? bandName : $"{bandName} - {albumTitle}";

            results.Add(new ListingItem(bandName, albumTitle, albumUrl, rawTitle, CurrentCategoryMediaType));
        }

        return results;
    }

    protected override async Task<AlbumParsedEvent> ParseAlbumDetails(string detailUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(detailUrl, cancellationToken);

        var bandName = GetNodeValue(htmlDocument, OsmoseProductionsSelectors.DetailBandName);
        var sku = GetNodeValue(htmlDocument, OsmoseProductionsSelectors.DetailSku)?.Split(':').Last().Trim();
        var name = ParseAlbumName(htmlDocument);
        var releaseDate = ParseReleaseDate(htmlDocument);
        var price = ParsePrice(htmlDocument);
        var photoUrl = ParsePhotoUrl(htmlDocument);
        var label = GetNodeValue(htmlDocument, OsmoseProductionsSelectors.DetailLabel);
        var press = GetNodeValue(htmlDocument, OsmoseProductionsSelectors.DetailPress)?.Split(':').Last().Trim();
        var description = htmlDocument.DocumentNode.SelectSingleNode(OsmoseProductionsSelectors.DetailDescription)?.InnerHtml;
        var statusText = ExtractStatusFromDetailPage(htmlDocument);
        var status = string.IsNullOrEmpty(statusText)
            ? null
            : AlbumParsingHelper.ParseAlbumStatus(statusText);

        return new AlbumParsedEvent
        {
            DistributorCode = DistributorCode,
            BandName = bandName,
            SKU = sku,
            Name = name,
            ReleaseDate = releaseDate,
            Price = price,
            PurchaseUrl = detailUrl,
            PhotoUrl = photoUrl,
            Label = label,
            Press = press,
            Description = description,
            Status = status
        };
    }

    protected override HtmlNode? FindNextPageLink(HtmlDocument htmlDocument)
    {
        var currentPageNode = htmlDocument.DocumentNode.SelectSingleNode(OsmoseProductionsSelectors.CurrentPageNode);

        if (currentPageNode != null && int.TryParse(currentPageNode.InnerText.Trim(), out int currentPageNumber))
        {
            var xpath = string.Format(OsmoseProductionsSelectors.NextPageTemplate, currentPageNumber + 1);
            return htmlDocument.DocumentNode.SelectSingleNode(xpath);
        }

        return null;
    }

    protected override Exception CreateParserException(string message, Exception? innerException = null)
    {
        return innerException != null
            ? new OsmoseProductionsParserException(message, innerException)
            : new OsmoseProductionsParserException(message);
    }

    protected override bool IsOwnException(Exception exception)
    {
        return exception is OsmoseProductionsParserException;
    }

    private string GetNodeValue(HtmlDocument document, string xPath)
    {
        var node = document.DocumentNode.SelectSingleNode(xPath);
        return node?.InnerText?.Trim();
    }

    private string ParseAlbumName(HtmlDocument htmlDocument)
    {
        var nameNode = htmlDocument.DocumentNode.SelectSingleNode(OsmoseProductionsSelectors.DetailAlbumName);
        var nameHtml = nameNode?.InnerHtml;
        var name = !string.IsNullOrEmpty(nameHtml)
            ? nameHtml.Split(new[] { "</a>&nbsp;" }, StringSplitOptions.None).Last().Trim()
            : null;

        return name;
    }

    private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
    {
        var releaseDateText = GetNodeValue(htmlDocument, OsmoseProductionsSelectors.DetailReleaseDate);
        return !string.IsNullOrEmpty(releaseDateText) ? AlbumParsingHelper.ParseYear(releaseDateText.Split(':').Last().Trim()) : DateTime.MinValue;
    }

    private float ParsePrice(HtmlDocument htmlDocument)
    {
        var priceText = GetNodeValue(htmlDocument, OsmoseProductionsSelectors.DetailPrice);
        priceText = priceText?.Replace("&nbsp;", " ").Replace("EUR", " ").Trim();

        return AlbumParsingHelper.ParsePrice(priceText);
    }

    private string ParsePhotoUrl(HtmlDocument htmlDocument)
    {
        var photoNode = htmlDocument.DocumentNode.SelectSingleNode(OsmoseProductionsSelectors.DetailPhoto);
        return photoNode?.GetAttributeValue("access_url", null);
    }

    private string ExtractStatusFromDetailPage(HtmlDocument htmlDocument)
    {
        var infoElements = htmlDocument.DocumentNode.SelectNodes(OsmoseProductionsSelectors.DetailStatus);

        if (infoElements != null)
        {
            foreach (var element in infoElements)
            {
                var innerText = element.InnerText.Trim();
                if (!string.IsNullOrEmpty(innerText))
                {
                    return innerText;
                }
            }
        }

        return string.Empty;
    }
}
