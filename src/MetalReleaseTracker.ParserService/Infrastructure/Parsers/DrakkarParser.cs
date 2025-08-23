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

public class DrakkarParser : IParser
{
    private readonly IHtmlDocumentLoader _htmlDocumentLoader;
    private readonly GeneralParserSettings _generalParserSettings;
    private readonly ILogger<DrakkarParser> _logger;
    private readonly Random _random = new();

    public DistributorCode DistributorCode => DistributorCode.Drakkar;

    public DrakkarParser(
        IHtmlDocumentLoader htmlDocumentLoader,
        IOptions<GeneralParserSettings> generalParserSettings,
        ILogger<DrakkarParser> logger)
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

        var productLinks = htmlDocument.DocumentNode.SelectNodes("//a[contains(@href, '/product/')]");
        if (productLinks != null)
        {
            foreach (var link in productLinks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var albumUrl = link.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrEmpty(albumUrl) || !albumUrl.Contains("/product/"))
                    continue;

                if (!albumUrl.StartsWith("http"))
                {
                    albumUrl = albumUrl.StartsWith("/") ? albumUrl : "/" + albumUrl;
                }

                results.Add((albumUrl, AlbumStatus.New));
            }
        }

        return results.Distinct().ToList();
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
            "//a[@class='next page-numbers']",
            "//a[contains(@class, 'next')]",
            "//a[contains(text(), 'Next')]",
            "//a[contains(text(), '→')]",
            "//a[contains(text(), '»')]",
            "//nav[@class='woocommerce-pagination']//a[last()]",
            "//div[@class='nav-links']//a[contains(@class, 'next')]"
        };

        foreach (var selector in nextPageSelectors)
        {
            var nextPageNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (nextPageNode != null)
            {
                var nextPageUrl = nextPageNode.GetAttributeValue("href", null);
                if (!string.IsNullOrEmpty(nextPageUrl))
                {
                    if (nextPageUrl != currentUrl)
                    {
                        _logger.LogInformation($"Next page found: {nextPageUrl}.");
                        return (nextPageUrl, true);
                    }
                }
            }
        }

        var paginationNumbers =
            htmlDocument.DocumentNode.SelectNodes("//a[@class='page-numbers' and not(contains(@class, 'current'))]");
        if (paginationNumbers != null)
        {
            var currentPageMatch = Regex.Match(currentUrl, @"/page/(\d+)/");
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

        if (currentUrl.Contains("?s=ukraine&post_type=product") && !currentUrl.Contains("/page/"))
        {
            var paginationExists = htmlDocument.DocumentNode.SelectSingleNode("//a[@class='page-numbers']") != null;
            if (paginationExists)
            {
                var nextPageUrl = "/page/2/?s=ukraine&post_type=product";
                _logger.LogInformation($"First pagination page constructed: {nextPageUrl}.");
                return (nextPageUrl, true);
            }
        }

        _logger.LogInformation("Next page not found - no pagination controls detected.");
        return (null, false);
    }

    private async Task<HtmlDocument> LoadHtmlDocument(string url, CancellationToken cancellationToken)
    {
        var htmlDocument = await _htmlDocumentLoader.LoadHtmlDocumentAsync(url, cancellationToken);

        if (htmlDocument?.DocumentNode == null)
        {
            var error = $"Failed to load or parse the HTML document {url}.";
            _logger.LogError(error);
            throw new DrakkarParserException(error);
        }

        return htmlDocument;
    }

    private string ParseBandName(HtmlDocument htmlDocument, string albumUrl)
    {
        var productTitleNode = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='product_title entry-title']") ??
                               htmlDocument.DocumentNode.SelectSingleNode("//h1[contains(@class, 'product_title')]") ??
                               htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='entry-title']");

        if (productTitleNode != null)
        {
            var title = HtmlEntity.DeEntitize(productTitleNode.InnerText.Trim());
            var dashIndex = title.IndexOf(" – ");
            if (dashIndex == -1) dashIndex = title.IndexOf(" - ");

            if (dashIndex > 0)
            {
                return title.Substring(0, dashIndex).Trim();
            }
        }

        var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//title");
        if (titleNode != null)
        {
            var title = HtmlEntity.DeEntitize(titleNode.InnerText);
            var parts = title.Split(new[] { " – ", " - " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[0].Trim();
            }
        }

        var urlParts = albumUrl.Split('/');
        var productSlug = urlParts[^2];
        var slugParts = productSlug.Split('-');
        if (slugParts.Length > 0)
        {
            return string.Join(" ", slugParts.Take(2)).ToUpper();
        }

        return null;
    }

    private string ParseAlbumName(HtmlDocument htmlDocument, string albumUrl)
    {
        var productTitleNode = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='product_title entry-title']") ??
                               htmlDocument.DocumentNode.SelectSingleNode("//h1[contains(@class, 'product_title')]") ??
                               htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='entry-title']");

        if (productTitleNode != null)
        {
            var title = HtmlEntity.DeEntitize(productTitleNode.InnerText.Trim());
            var parts = title.Split(new[] { " – ", " - " }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 2)
            {
                return parts[1].Trim();
            }
            else if (parts.Length == 1)
            {
                return parts[0].Trim();
            }
        }

        var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//title");
        if (titleNode != null)
        {
            var title = HtmlEntity.DeEntitize(titleNode.InnerText);
            var parts = title.Split(new[] { " – ", " - " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 1)
            {
                return parts[1].Trim();
            }
        }

        return null;
    }

    private string ParseSku(HtmlDocument htmlDocument)
    {
        var skuNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='sku']") ??
                      htmlDocument.DocumentNode.SelectSingleNode("//span[@class='sku_wrapper']//span[@class='sku']");

        if (skuNode != null)
        {
            return skuNode.InnerText.Trim();
        }

        var metaSkuNode = htmlDocument.DocumentNode.SelectSingleNode("//meta[@property='product:retailer_item_id']");
        if (metaSkuNode != null)
        {
            return metaSkuNode.GetAttributeValue("content", null);
        }

        return null;
    }

    private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
    {
        var selectors = new[]
        {
            "//div[@class='woocommerce-product-details__short-description']",
            "//div[contains(@class, 'short-description')]",
            "//div[@class='product-description']",
            "//div[contains(@class, 'product-details')]"
        };

        foreach (var selector in selectors)
        {
            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (descriptionNode != null)
            {
                var text = descriptionNode.InnerText;
                var yearMatch = Regex.Match(text, @"\b(19|20)\d{2}\b");
                if (yearMatch.Success && int.TryParse(yearMatch.Value, out int year))
                {
                    return new DateTime(year, 1, 1);
                }
            }
        }

        return DateTime.MinValue;
    }

    private float ParsePrice(HtmlDocument htmlDocument)
    {
        var priceSelectors = new[]
        {
            "//p[@class='price']//bdi",
            "//span[@class='price']//bdi",
            "//p[@class='price']//span[@class='woocommerce-Price-amount amount']//bdi",
            "//span[@class='price']//span[@class='woocommerce-Price-amount amount']//bdi",
            "//p[@class='price']//span[@class='woocommerce-Price-amount amount']",
            "//span[@class='price']//span[@class='woocommerce-Price-amount amount']",
            "//bdi[contains(text(), '€')]",
            "//span[@class='amount']//bdi",
            "//p[@class='price']",
            "//span[@class='price']"
        };

        foreach (var selector in priceSelectors)
        {
            var priceNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (priceNode != null)
            {
                var priceText = priceNode.InnerText
                    .Replace("€", string.Empty)
                    .Replace(",", ".")
                    .Replace("Original price was:", string.Empty)
                    .Replace("Current price is:", string.Empty)
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
            "//div[@class='woocommerce-product-gallery__image']//img",
            "//figure[@class='woocommerce-product-gallery__wrapper']//img",
            "//div[contains(@class, 'product-image')]//img",
            "//div[contains(@class, 'product-gallery')]//img",
            "//img[contains(@class, 'wp-post-image')]"
        };

        foreach (var selector in imageSelectors)
        {
            var imageNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (imageNode != null)
            {
                var srcSet = imageNode.GetAttributeValue("srcset", null);
                if (!string.IsNullOrEmpty(srcSet))
                {
                    var urls = srcSet.Split(',');
                    var firstUrl = urls.FirstOrDefault()?.Split(' ')[0]?.Trim();
                    if (!string.IsNullOrEmpty(firstUrl))
                    {
                        return firstUrl;
                    }
                }

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
        var productTitle = htmlDocument.DocumentNode.SelectSingleNode("//h1")?.InnerText?.ToUpper() ?? string.Empty;

        var combinedText = $"{title} {url} {productTitle}";

        if (combinedText.Contains("LP") && !combinedText.Contains("HELP"))
            return AlbumMediaType.LP;
        if (combinedText.Contains("VINYL"))
            return AlbumMediaType.LP;
        if (combinedText.Contains("TAPE") || combinedText.Contains("CASSETTE"))
            return AlbumMediaType.Tape;
        if (combinedText.Contains("CD") || combinedText.Contains("DIGICD") ||
            combinedText.Contains("DIGI") || combinedText.Contains("MCD") ||
            combinedText.Contains("MINI"))
            return AlbumMediaType.CD;

        var attributeNodes =
            htmlDocument.DocumentNode.SelectNodes(
                "//table[@class='woocommerce-product-attributes shop_attributes']//td");
        if (attributeNodes != null)
        {
            foreach (var node in attributeNodes)
            {
                var text = node.InnerText.ToUpper();
                if (text.Contains("VINYL") || text.Contains("LP"))
                    return AlbumMediaType.LP;
                if (text.Contains("CD"))
                    return AlbumMediaType.CD;
                if (text.Contains("CASSETTE") || text.Contains("TAPE"))
                    return AlbumMediaType.Tape;
            }
        }

        return null;
    }

    private string ParseLabel(HtmlDocument htmlDocument)
    {
        var descriptionSelectors = new[]
        {
            "//div[@class='woocommerce-product-details__short-description']",
            "//div[contains(@class, 'short-description')]",
            "//div[@class='product-description']"
        };

        foreach (var selector in descriptionSelectors)
        {
            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (descriptionNode != null)
            {
                var text = descriptionNode.InnerText;
                var labelMatch = Regex.Match(text, @"Label\s*[:\s]\s*([^\n\r.]+)", RegexOptions.IgnoreCase);
                if (labelMatch.Success)
                {
                    return labelMatch.Groups[1].Value.Trim();
                }
            }
        }

        var attributeNodes =
            htmlDocument.DocumentNode.SelectNodes(
                "//table[@class='woocommerce-product-attributes shop_attributes']//tr");
        if (attributeNodes != null)
        {
            foreach (var row in attributeNodes)
            {
                var header = row.SelectSingleNode(".//th")?.InnerText?.Trim().ToLower();
                if (header != null && header.Contains("label"))
                {
                    var value = row.SelectSingleNode(".//td")?.InnerText?.Trim();
                    if (!string.IsNullOrEmpty(value))
                    {
                        return value;
                    }
                }
            }
        }

        return null;
    }

    private string ParsePress(HtmlDocument htmlDocument)
    {
        var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='woocommerce-product-details__short-description']");
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
        var shortDescSelectors = new[]
        {
            "//div[@class='woocommerce-product-details__short-description']",
            "//div[contains(@class, 'short-description')]",
            "//div[@class='product-description']"
        };

        foreach (var selector in shortDescSelectors)
        {
            var descNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (descNode != null)
            {
                return HtmlEntity.DeEntitize(descNode.InnerText.Trim());
            }
        }

        var fullDescSelectors = new[]
        {
            "//div[@id='tab-description']",
            "//div[contains(@class, 'woocommerce-Tabs-panel--description')]"
        };

        foreach (var selector in fullDescSelectors)
        {
            var descNode = htmlDocument.DocumentNode.SelectSingleNode(selector);
            if (descNode != null)
            {
                return HtmlEntity.DeEntitize(descNode.InnerText.Trim());
            }
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