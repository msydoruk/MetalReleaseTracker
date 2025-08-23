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

public class DarkThanBlackRecordsParser : IParser
{
    private readonly IHtmlDocumentLoader _htmlDocumentLoader;
    private readonly GeneralParserSettings _generalParserSettings;
    private readonly ILogger<DarkThanBlackRecordsParser> _logger;
    private readonly Random _random = new();

    public DistributorCode DistributorCode => DistributorCode.DarkThanBlackRecords;

    public DarkThanBlackRecordsParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<DarkThanBlackRecordsParser> logger)
    {
        _htmlDocumentLoader = htmlDocumentLoader;
        _generalParserSettings = generalParserSettings.Value;
        _logger = logger;
    }

    public async Task<PageParsedResult> ParseAsync(string parsingUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(parsingUrl, cancellationToken);
        var albumUrlsWithStatus = ParseAlbumUrlsWithStatus(htmlDocument, cancellationToken);

        var parsedAlbums = new List<AlbumParsedEvent>();
        foreach (var (url, status) in albumUrlsWithStatus)
        {
            AlbumParsedEvent albumParsedEvent = await ParseAlbumDetails(url, cancellationToken);
            albumParsedEvent.Status = status;
            parsedAlbums.Add(albumParsedEvent);
            _logger.LogInformation($"Parsed album: {albumParsedEvent.Name} by {albumParsedEvent.BandName}.");

            await DelayBetweenRequests(cancellationToken);
        }

        (string nextPageUrl, bool hasMorePages) =
            GetNextPageUrl(htmlDocument, parsingUrl, albumUrlsWithStatus.Count > 0);

        return new PageParsedResult
        {
            ParsedAlbums = parsedAlbums,
            NextPageUrl = hasMorePages ? nextPageUrl : null
        };
    }

    private List<(string Url, AlbumStatus? Status)> ParseAlbumUrlsWithStatus(HtmlDocument htmlDocument,
        CancellationToken cancellationToken)
    {
        var results = new List<(string Url, AlbumStatus? Status)>();

        var productContainers = htmlDocument.DocumentNode.SelectNodes("//td[@class='productListing-data']");
        if (productContainers != null)
        {
            foreach (var container in productContainers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var linkNode = container.SelectSingleNode(".//a[contains(@href, 'product_info')]");
                if (linkNode != null)
                {
                    var albumUrl = linkNode.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrEmpty(albumUrl))
                    {
                        if (!albumUrl.StartsWith("http"))
                        {
                            albumUrl = albumUrl.StartsWith("/") ? albumUrl : "/" + albumUrl;
                        }

                        var statusText = ExtractStockStatus(container);
                        var status = ParseStockStatus(statusText);

                        results.Add((albumUrl, status));
                    }
                }
            }
        }

        return results.Distinct().ToList();
    }

    private string ExtractStockStatus(HtmlNode container)
    {
        var stockNode = container.SelectSingleNode(".//span[contains(@class, 'stock')]") ??
                        container.SelectSingleNode(".//td[contains(text(), 'Stock')]") ??
                        container.SelectSingleNode(".//font[contains(text(), 'Stock')]");

        return stockNode?.InnerText?.Trim() ?? string.Empty;
    }

    private AlbumStatus ParseStockStatus(string stockText)
    {
        if (string.IsNullOrEmpty(stockText))
            return AlbumStatus.New;

        var lowerStock = stockText.ToLower();

        if (lowerStock.Contains("out of stock") || lowerStock.Contains("sold out"))
            return AlbumStatus.New;
        if (lowerStock.Contains("pre-order") || lowerStock.Contains("preorder"))
            return AlbumStatus.PreOrder;
        if (lowerStock.Contains("restock"))
            return AlbumStatus.Restock;

        return AlbumStatus.New;
    }

    private async Task<AlbumParsedEvent> ParseAlbumDetails(string albumUrl, CancellationToken cancellationToken)
    {
        var htmlDocument = await LoadHtmlDocument(albumUrl, cancellationToken);

        var bandName = ParseBandName(htmlDocument, albumUrl);
        var sku = ParseSku(htmlDocument);
        var name = ParseAlbumName(htmlDocument, albumUrl);
        var releaseDate = ParseReleaseDate(htmlDocument);
        var price = ParsePrice(htmlDocument);
        var photoUrl = ParsePhotoUrl(htmlDocument);
        var media = ParseMediaType(htmlDocument, albumUrl);
        var label = ParseLabel(htmlDocument);
        var press = ParsePress(htmlDocument);
        var description = ParseDescription(htmlDocument);

        return new AlbumParsedEvent
        {
            DistributorCode = DistributorCode,
            BandName = bandName,
            SKU = sku,
            Name = name,
            ReleaseDate = releaseDate,
            Price = price,
            PurchaseUrl = albumUrl,
            PhotoUrl = photoUrl,
            Media = media,
            Label = label,
            Press = press,
            Description = description
        };
    }

    private (string nextPageUrl, bool hasMorePages) GetNextPageUrl(HtmlDocument htmlDocument, string currentUrl, bool hasProductsOnCurrentPage)
    {
        var nextPageSelectors = new[]
        {
            "//a[contains(text(), 'Next')]",
            "//a[contains(text(), '»')]",
            "//a[contains(text(), '&gt;')]",
            "//td[@class='smallText']//a[last()]"
        };

        foreach (var selector in nextPageSelectors)
        {
            var nextPageNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (nextPageNode != null)
            {
                var nextPageUrl = nextPageNode.GetAttributeValue("href", null);
                if (!string.IsNullOrEmpty(nextPageUrl))
                {
                    if (nextPageUrl.StartsWith("/"))
                    {
                        nextPageUrl = nextPageUrl;
                    }

                    if (nextPageUrl != currentUrl)
                    {
                        _logger.LogInformation($"Next page found: {nextPageUrl}.");
                        return (nextPageUrl, true);
                    }
                }
            }
        }

        var paginationNumbers = htmlDocument.DocumentNode.SelectNodes("//td[@class='smallText']//a");
        if (paginationNumbers != null)
        {
            var currentPageMatch = Regex.Match(currentUrl, @"page=(\d+)");
            int currentPageNumber = 1;

            if (currentPageMatch.Success)
            {
                currentPageNumber = int.Parse(currentPageMatch.Groups[1].Value);
            }

            foreach (var pageNode in paginationNumbers)
            {
                var pageText = pageNode.InnerText.Trim();
                if (int.TryParse(pageText, out int pageNumber) && pageNumber > currentPageNumber)
                {
                    var nextPageUrl = pageNode.GetAttributeValue("href", null);
                    if (!string.IsNullOrEmpty(nextPageUrl))
                    {
                        _logger.LogInformation($"Next page found from pagination: {nextPageUrl}.");
                        return (nextPageUrl, true);
                    }
                }
            }
        }

        _logger.LogInformation("Next page not found - no pagination controls detected.");
        return (null, false);
    }

    private string GetNodeValue(HtmlDocument document, string xPath)
    {
        var node = document.DocumentNode.SelectSingleNode(xPath);
        return node?.InnerText?.Trim();
    }

    private async Task<HtmlDocument> LoadHtmlDocument(string url, CancellationToken cancellationToken)
    {
        var htmlDocument = await _htmlDocumentLoader.LoadHtmlDocumentAsync(url, cancellationToken);

        if (htmlDocument?.DocumentNode == null)
        {
            var error = $"Failed to load or parse the HTML document {url}.";
            _logger.LogError(error);
            throw new DarkerThanBlackRecordsParserExeption(error);
        }

        return htmlDocument;
    }

    private string ParseBandName(HtmlDocument htmlDocument, string albumUrl)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//td[@class='pageHeading']") ??
                        htmlDocument.DocumentNode.SelectSingleNode("//h1") ??
                        htmlDocument.DocumentNode.SelectSingleNode("//title");

        if (titleNode != null)
        {
            var title = HtmlEntity.DeEntitize(titleNode.InnerText.Trim());
            var dashIndex = title.IndexOf(" - ");
            if (dashIndex > 0)
            {
                return title.Substring(0, dashIndex).Trim();
            }

            var parts = title.Split(new[] { ":", " – " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[0].Trim();
            }
        }

        var productNameNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='productNameText']");
        if (productNameNode != null)
        {
            var productName = HtmlEntity.DeEntitize(productNameNode.InnerText.Trim());
            var parts = productName.Split(new[] { " - ", ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[0].Trim();
            }
        }

        return null;
    }

    private string ParseAlbumName(HtmlDocument htmlDocument, string albumUrl)
    {
        var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//td[@class='pageHeading']") ??
                        htmlDocument.DocumentNode.SelectSingleNode("//h1");

        if (titleNode != null)
        {
            var title = HtmlEntity.DeEntitize(titleNode.InnerText.Trim());
            var parts = title.Split(new[] { " - ", ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                return parts[1].Trim();
            }
        }

        var productNameNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='productNameText']");
        if (productNameNode != null)
        {
            var productName = HtmlEntity.DeEntitize(productNameNode.InnerText.Trim());
            var parts = productName.Split(new[] { " - ", ":" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                return parts[1].Trim();
            }

            return productName;
        }

        return null;
    }

    private string ParseSku(HtmlDocument htmlDocument)
    {
        var skuNode = htmlDocument.DocumentNode.SelectSingleNode("//span[contains(text(), 'Art.Nr.:')]") ??
                      htmlDocument.DocumentNode.SelectSingleNode("//span[contains(text(), 'Article No.:')]") ??
                      htmlDocument.DocumentNode.SelectSingleNode("//td[contains(text(), 'Product ID:')]");

        if (skuNode != null)
        {
            var skuText = skuNode.InnerText;
            var match = Regex.Match(skuText, @"(?:Art\.Nr\.|Article No\.|Product ID:)\s*(\w+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
    {
        var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='productDescriptionText']") ??
                              htmlDocument.DocumentNode.SelectSingleNode("//td[@class='productInfoDescription']");

        if (descriptionNode != null)
        {
            var text = descriptionNode.InnerText;
            var yearMatch = Regex.Match(text, @"\b(19|20)\d{2}\b");
            if (yearMatch.Success && int.TryParse(yearMatch.Value, out int year))
            {
                return new DateTime(year, 1, 1);
            }
        }

        return DateTime.MinValue;
    }

    private float ParsePrice(HtmlDocument htmlDocument)
    {
        var priceSelectors = new[]
        {
            "//span[@class='productSpecialPrice']",
            "//span[@class='productPriceValue']",
            "//td[@class='productInfoPrice']",
            "//span[contains(@class, 'price')]"
        };

        foreach (var selector in priceSelectors)
        {
            var priceNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (priceNode != null)
            {
                var priceText = priceNode.InnerText
                    .Replace("EUR", string.Empty)
                    .Replace("€", string.Empty)
                    .Replace(",", ".")
                    .Trim();

                var priceMatch = Regex.Match(priceText, @"(\d+[.,]?\d*)");
                if (priceMatch.Success)
                {
                    var cleanPrice = priceMatch.Groups[1].Value.Replace(",", ".");
                    var price = AlbumParsingHelper.ParsePrice(cleanPrice);
                    if (price > 0)
                    {
                        return price;
                    }
                }
            }
        }

        return 0f;
    }

    private string ParsePhotoUrl(HtmlDocument htmlDocument)
    {
        var imageSelectors = new[]
        {
            "//td[@class='productInfoImage']//img",
            "//span[@class='productImageLarge']//img",
            "//img[contains(@src, 'product')]"
        };

        foreach (var selector in imageSelectors)
        {
            var imageNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (imageNode != null)
            {
                var src = imageNode.GetAttributeValue("src", null);
                if (!string.IsNullOrEmpty(src))
                {
                    return src;
                }
            }
        }

        return null;
    }

    private AlbumMediaType? ParseMediaType(HtmlDocument htmlDocument, string albumUrl)
    {
        var title = htmlDocument.DocumentNode.SelectSingleNode("//title")?.InnerText?.ToUpper() ?? string.Empty;
        var url = albumUrl.ToUpper();
        var productTitle =
            htmlDocument.DocumentNode.SelectSingleNode("//td[@class='pageHeading']")?.InnerText?.ToUpper() ??
            string.Empty;

        var combinedText = $"{title} {url} {productTitle}";

        if (combinedText.Contains("LP") || combinedText.Contains("VINYL"))
            return AlbumMediaType.LP;
        if (combinedText.Contains("TAPE") || combinedText.Contains("CASSETTE"))
            return AlbumMediaType.Tape;
        if (combinedText.Contains("CD"))
            return AlbumMediaType.CD;

        return null;
    }

    private string ParseLabel(HtmlDocument htmlDocument)
    {
        var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='productDescriptionText']") ??
                              htmlDocument.DocumentNode.SelectSingleNode("//td[@class='productInfoDescription']");

        if (descriptionNode != null)
        {
            var text = descriptionNode.InnerText;
            var labelMatch = Regex.Match(text, @"Label[:\s]\s*([^\n\r.]+)", RegexOptions.IgnoreCase);
            if (labelMatch.Success)
            {
                return labelMatch.Groups[1].Value.Trim();
            }
        }

        return null;
    }

    private string ParsePress(HtmlDocument htmlDocument)
    {
        var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='productDescriptionText']") ??
                              htmlDocument.DocumentNode.SelectSingleNode("//td[@class='productInfoDescription']");

        if (descriptionNode != null)
        {
            var text = descriptionNode.InnerText;
            var limitedMatch = Regex.Match(text, @"Limited\s+edition\s+to\s+(\d+)\s+copies", RegexOptions.IgnoreCase);
            if (limitedMatch.Success)
            {
                return limitedMatch.Value;
            }
        }

        return "None";
    }

    private string ParseDescription(HtmlDocument htmlDocument)
    {
        var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='productDescriptionText']") ??
                              htmlDocument.DocumentNode.SelectSingleNode("//td[@class='productInfoDescription']");

        if (descriptionNode != null)
        {
            return HtmlEntity.DeEntitize(descriptionNode.InnerText.Trim());
        }

        return null;
    }

    private async Task DelayBetweenRequests(CancellationToken cancellationToken)
    {
        var delaySeconds = _random.Next(_generalParserSettings.MinDelayBetweenRequestsSeconds,
            _generalParserSettings.MaxDelayBetweenRequestsSeconds);

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
    }
}