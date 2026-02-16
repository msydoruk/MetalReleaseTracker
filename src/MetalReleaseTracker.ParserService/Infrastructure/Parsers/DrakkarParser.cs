using System.Text.Json;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.Results;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers;

public class DrakkarParser : IListingParser, IAlbumDetailParser
    {
        private readonly IHttpRequestService _httpRequestService;
        private readonly IHtmlDocumentLoader _htmlDocumentLoader;
        private readonly GeneralParserSettings _generalParserSettings;
        private readonly ILogger<DrakkarParser> _logger;
        private readonly Random _random = new();

        public DistributorCode DistributorCode => DistributorCode.Drakkar;

        public DrakkarParser(
            IHttpRequestService httpRequestService,
            IHtmlDocumentLoader htmlDocumentLoader,
            IOptions<GeneralParserSettings> generalSettings,
            ILogger<DrakkarParser> logger)
        {
            _httpRequestService = httpRequestService;
            _htmlDocumentLoader = htmlDocumentLoader;
            _generalParserSettings = generalSettings.Value;
            _logger = logger;
        }

        public async Task<ListingPageResult> ParseListingsAsync(string url, CancellationToken cancellationToken)
        {
            var baseUri = new Uri(url);
            var ajaxUrl = $"{baseUri.Scheme}://{baseUri.Host}{AjaxPath}";

            _logger.LogInformation("Fetching all Drakkar products via AJAX endpoint.");

            var html = await FetchAlphabetPageAsync(ajaxUrl, 'A', cancellationToken);

            if (string.IsNullOrEmpty(html) || html == "0")
            {
                _logger.LogInformation("No products returned from Drakkar AJAX endpoint.");
                return new ListingPageResult
                {
                    Listings = new List<ListingItem>(),
                    NextPageUrl = null
                };
            }

            var processedUrls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var listings = ParseProductsFromHtml(html, processedUrls, cancellationToken);

            _logger.LogInformation("Fetched {Count} Drakkar products.", listings.Count);

            return new ListingPageResult
            {
                Listings = listings,
                NextPageUrl = null
            };
        }

        public async Task<AlbumParsedEvent> ParseAlbumDetailAsync(string detailUrl, CancellationToken cancellationToken)
        {
            await DelayBetweenRequests(cancellationToken);
            return await ParseAlbumDetails(detailUrl, cancellationToken);
        }

        private async Task<string> FetchAlphabetPageAsync(string ajaxUrl, char letter, CancellationToken cancellationToken)
        {
            var url = $"{ajaxUrl}?action=filter_products_by_alphabet&letter={letter}&category=";
            var headers = new Dictionary<string, string>
            {
                { "X-Requested-With", "XMLHttpRequest" },
                { "Referer", "https://www.drakkar666.com/shop/" },
                { "Accept", "text/html, */*; q=0.01" }
            };

            return await _httpRequestService.GetStringWithUserAgentAsync(url, headers, cancellationToken);
        }

        private List<ListingItem> ParseProductsFromHtml(string html, HashSet<string> processedUrls, CancellationToken cancellationToken)
        {
            var results = new List<ListingItem>();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var productNodes = doc.DocumentNode.SelectNodes("//li[contains(@class,'product-warp-item')]")
                ?? doc.DocumentNode.SelectNodes("//div[contains(@class,'type-product')]");

            if (productNodes == null)
            {
                return results;
            }

            foreach (var productNode in productNodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var anchorNode = productNode.SelectSingleNode(".//a[contains(@href,'/product/')]");
                if (anchorNode == null)
                {
                    continue;
                }

                var href = anchorNode.GetAttributeValue("href", string.Empty).Trim();
                if (string.IsNullOrEmpty(href) || !processedUrls.Add(href))
                {
                    continue;
                }

                var titleNode = productNode.SelectSingleNode(".//a[contains(@class,'woocommerce-loop-product__title')]");
                var titleText = titleNode != null
                    ? HtmlEntity.DeEntitize(titleNode.InnerText?.Trim() ?? string.Empty)
                    : string.Empty;

                var (bandName, albumTitle, _) = ParseTitleParts(titleText);

                results.Add(new ListingItem(bandName, albumTitle, href, titleText));
            }

            return results;
        }

        private async Task<AlbumParsedEvent> ParseAlbumDetails(string albumUrl, CancellationToken cancellationToken)
        {
            var htmlDocument = await LoadHtmlDocument(albumUrl, cancellationToken);
            var jsonLd = ExtractJsonLd(htmlDocument);

            var (bandName, albumName, mediaTypeRaw) = ParseTitle(htmlDocument);
            var sku = ParseSku(jsonLd, htmlDocument, albumUrl);
            var price = ParsePrice(htmlDocument);
            var photoUrl = ParsePhotoUrl(jsonLd, htmlDocument);
            var media = ParseDrakkarMediaType(mediaTypeRaw);
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
                PurchaseUrl = albumUrl,
                PhotoUrl = photoUrl,
                Media = media,
                Label = label,
                Press = sku,
                Description = description,
                Status = null
            };
        }

        private JsonElement? ExtractJsonLd(HtmlDocument htmlDocument)
        {
            var scriptNodes = htmlDocument.DocumentNode.SelectNodes("//script[@type='application/ld+json']");
            if (scriptNodes == null)
            {
                return null;
            }

            foreach (var scriptNode in scriptNodes)
            {
                var json = scriptNode.InnerText?.Trim();
                if (string.IsNullOrEmpty(json))
                {
                    continue;
                }

                try
                {
                    var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("@type", out var typeElement) &&
                        typeElement.GetString() == "Product")
                    {
                        return root;
                    }

                    if (root.TryGetProperty("@graph", out var graph) &&
                        graph.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in graph.EnumerateArray())
                        {
                            if (item.TryGetProperty("@type", out var itemType) &&
                                itemType.GetString() == "Product")
                            {
                                return item;
                            }
                        }
                    }
                }
                catch (JsonException)
                {
                }
            }

            return null;
        }

        private (string BandName, string AlbumName, string MediaTypeRaw) ParseTitle(HtmlDocument htmlDocument)
        {
            var titleNode = htmlDocument.DocumentNode.SelectSingleNode("//h1[contains(@class,'product_title')]");
            var titleText = titleNode?.InnerText?.Trim();

            if (string.IsNullOrEmpty(titleText))
            {
                return (string.Empty, string.Empty, string.Empty);
            }

            titleText = HtmlEntity.DeEntitize(titleText);
            return ParseTitleParts(titleText);
        }

        private AlbumMediaType? ParseDrakkarMediaType(string rawMediaType)
        {
            if (string.IsNullOrWhiteSpace(rawMediaType))
            {
                return null;
            }

            var upper = rawMediaType.ToUpper();

            if (upper.Contains("FANZINE"))
            {
                return null;
            }

            if (upper.Contains("LP"))
            {
                return AlbumMediaType.LP;
            }

            if (upper.Contains("CD") || upper.Contains("MCD"))
            {
                return AlbumMediaType.CD;
            }

            if (upper.Contains("TAPE"))
            {
                return AlbumMediaType.Tape;
            }

            return null;
        }

        private float ParsePrice(HtmlDocument htmlDocument)
        {
            var priceNode = htmlDocument.DocumentNode.SelectSingleNode("//p[contains(@class,'price')]//bdi")
                ?? htmlDocument.DocumentNode.SelectSingleNode("//span[contains(@class,'woocommerce-Price-amount')]//bdi");

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

            var skuNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='sku']");
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

            var imgNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[contains(@class,'woocommerce-product-gallery')]//img");

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
            var lines = GetStructuredTextLines(htmlDocument,
                "//div[contains(@class,'woocommerce-product-details__short-description')]");

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

            var attrLines = GetStructuredTextLines(htmlDocument,
                "//table[contains(@class,'woocommerce-product-attributes')]");

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

            var lines = GetStructuredTextLines(htmlDocument,
                "//div[contains(@class,'woocommerce-product-details__short-description')]");

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
            var allLines = GetStructuredTextLines(htmlDocument,
                "//div[contains(@class,'woocommerce-product-details__short-description')]");
            allLines.AddRange(GetStructuredTextLines(htmlDocument,
                "//table[contains(@class,'woocommerce-product-attributes')]"));

            foreach (var line in allLines)
            {
                var match = Regex.Match(line, @"Release Year\s*:\s*(\d{4})", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return AlbumParsingHelper.ParseYear(match.Groups[1].Value);
                }
            }

            var categoryLinks = htmlDocument.DocumentNode.SelectNodes("//span[@class='posted_in']//a");
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
                    return StripHtml(description);
                }
            }

            var descNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[contains(@class,'woocommerce-product-details__short-description')]");

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

        private async Task DelayBetweenRequests(CancellationToken cancellationToken)
        {
            var delaySeconds = _random.Next(_generalParserSettings.MinDelayBetweenRequestsSeconds,
                _generalParserSettings.MaxDelayBetweenRequestsSeconds);

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
        }

        private static readonly char[] TitleSeparators = ['\u2013', '\u2014', '-'];
        private static readonly string AjaxPath = "/wp-admin/admin-ajax.php";

        private static string StripHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return html;
            }

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var text = doc.DocumentNode.InnerText ?? string.Empty;
            text = HtmlEntity.DeEntitize(text);
            text = Regex.Replace(text, @"[\x00-\x08\x0B\x0C\x0E-\x1F]", string.Empty);

            return text.Trim();
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
