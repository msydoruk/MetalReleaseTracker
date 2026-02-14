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
        private static readonly char[] TitleSeparators = { '\u2013', '\u2014', '-' };

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
            var productUrls = ParseProductUrls(htmlDocument, cancellationToken);

            var parsedAlbums = new List<AlbumParsedEvent>();
            foreach (var url in productUrls)
            {
                var albumParsedEvent = await ParseAlbumDetails(url, cancellationToken);
                parsedAlbums.Add(albumParsedEvent);
                _logger.LogInformation($"Parsed album: {albumParsedEvent.Name} by {albumParsedEvent.BandName}.");

                await DelayBetweenRequests(cancellationToken);
            }

            var (nextPageUrl, hasMorePages) = GetNextPageUrl(htmlDocument, parsingUrl);

            return new PageParsedResult
            {
                ParsedAlbums = parsedAlbums,
                NextPageUrl = hasMorePages ? nextPageUrl : null
            };
        }

        private List<string> ParseProductUrls(HtmlDocument htmlDocument, CancellationToken cancellationToken)
        {
            var urls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var anchorNodes = htmlDocument.DocumentNode.SelectNodes("//a[contains(@href, '/product/')]");

            if (anchorNodes != null)
            {
                foreach (var anchorNode in anchorNodes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var href = anchorNode.GetAttributeValue("href", string.Empty).Trim();
                    if (!string.IsNullOrEmpty(href) && href.Contains("/product/") && !href.Contains("add-to-cart"))
                    {
                        urls.Add(href);
                    }
                }
            }

            return urls.ToList();
        }

        private async Task<AlbumParsedEvent> ParseAlbumDetails(string albumUrl, CancellationToken cancellationToken)
        {
            var htmlDocument = await LoadHtmlDocument(albumUrl, cancellationToken);

            var (bandName, albumName, mediaTypeRaw) = ParseTitle(htmlDocument);
            var sku = ParseSku(htmlDocument, albumUrl);
            var price = ParsePrice(htmlDocument);
            var photoUrl = ParsePhotoUrl(htmlDocument);
            var media = ParseDrakkarMediaType(mediaTypeRaw);
            var label = ParseLabel(htmlDocument);
            var genre = ParseGenre(htmlDocument);
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
                PurchaseUrl = albumUrl,
                PhotoUrl = photoUrl,
                Media = media,
                Label = label,
                Press = sku,
                Description = description,
                Status = null
            };
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
                return (bandName, mediaTypeRaw, string.Empty);
            }

            var albumName = string.Join(" - ", parts[1..^1]).Trim();
            return (bandName, albumName, mediaTypeRaw);
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

        private string ParseSku(HtmlDocument htmlDocument, string albumUrl)
        {
            var skuNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='sku']");
            var sku = skuNode?.InnerText?.Trim();

            if (!string.IsNullOrEmpty(sku))
            {
                return sku;
            }

            var slugMatch = Regex.Match(albumUrl, @"/product/([^/]+)");
            return slugMatch.Success ? slugMatch.Groups[1].Value : albumUrl;
        }

        private string ParsePhotoUrl(HtmlDocument htmlDocument)
        {
            var galleryLink = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[contains(@class,'woocommerce-product-gallery__image')]//a");
            var href = galleryLink?.GetAttributeValue("href", null);

            if (!string.IsNullOrEmpty(href))
            {
                return href;
            }

            var imgNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[contains(@class,'woocommerce-product-gallery__image')]//img");
            var src = imgNode?.GetAttributeValue("src", null);

            if (!string.IsNullOrEmpty(src))
            {
                return Regex.Replace(src, @"-\d+x\d+(?=\.\w+$)", string.Empty);
            }

            return string.Empty;
        }

        private string ParseGenre(HtmlDocument htmlDocument)
        {
            var attributeRows = htmlDocument.DocumentNode.SelectNodes(
                "//table[contains(@class,'woocommerce-product-attributes')]//tr");

            if (attributeRows != null)
            {
                foreach (var row in attributeRows)
                {
                    var text = HtmlEntity.DeEntitize(row.InnerText?.Trim() ?? string.Empty);
                    if (!text.Contains("Origin", StringComparison.OrdinalIgnoreCase)
                        && !text.Contains("Label", StringComparison.OrdinalIgnoreCase)
                        && !text.Contains("Release Year", StringComparison.OrdinalIgnoreCase)
                        && !text.Contains("Weight", StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(text))
                    {
                        return text;
                    }
                }
            }

            var shortDesc = GetShortDescriptionText(htmlDocument);
            if (!string.IsNullOrEmpty(shortDesc))
            {
                var lines = shortDesc.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (!string.IsNullOrEmpty(trimmed)
                        && !trimmed.StartsWith("Origin", StringComparison.OrdinalIgnoreCase)
                        && !trimmed.StartsWith("Label", StringComparison.OrdinalIgnoreCase)
                        && !trimmed.StartsWith("Release", StringComparison.OrdinalIgnoreCase))
                    {
                        return trimmed;
                    }
                }
            }

            return string.Empty;
        }

        private string ParseLabel(HtmlDocument htmlDocument)
        {
            var pageText = HtmlEntity.DeEntitize(htmlDocument.DocumentNode.InnerText ?? string.Empty);
            var match = Regex.Match(pageText, @"Label\s*:\s*(.+?)(?:\n|\r|$)", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            var attributeRows = htmlDocument.DocumentNode.SelectNodes(
                "//table[contains(@class,'woocommerce-product-attributes')]//tr");

            if (attributeRows != null)
            {
                foreach (var row in attributeRows)
                {
                    var text = HtmlEntity.DeEntitize(row.InnerText?.Trim() ?? string.Empty);
                    if (text.Contains("Label", StringComparison.OrdinalIgnoreCase))
                    {
                        var labelMatch = Regex.Match(text, @"Label\s*:\s*(.+)", RegexOptions.IgnoreCase);
                        if (labelMatch.Success)
                        {
                            return labelMatch.Groups[1].Value.Trim();
                        }
                    }
                }
            }

            return string.Empty;
        }

        private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
        {
            var pageText = HtmlEntity.DeEntitize(htmlDocument.DocumentNode.InnerText ?? string.Empty);
            var match = Regex.Match(pageText, @"Release Year\s*:\s*(\d{4})", RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return AlbumParsingHelper.ParseYear(match.Groups[1].Value);
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

        private string ParseDescription(HtmlDocument htmlDocument)
        {
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

            var tabDesc = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='tab-description']");
            if (tabDesc != null)
            {
                return HtmlEntity.DeEntitize(tabDesc.InnerText?.Trim() ?? string.Empty);
            }

            return string.Empty;
        }

        private (string NextPageUrl, bool HasMorePages) GetNextPageUrl(HtmlDocument htmlDocument, string currentUrl)
        {
            var currentPage = 1;
            var pageMatch = Regex.Match(currentUrl, @"/page/(\d+)/");
            if (pageMatch.Success)
            {
                currentPage = int.Parse(pageMatch.Groups[1].Value);
            }

            var nextPage = currentPage + 1;
            var nextPageNode = htmlDocument.DocumentNode.SelectSingleNode($"//a[contains(@href, '/page/{nextPage}/')]");

            if (nextPageNode != null)
            {
                var nextPageUrl = nextPageNode.GetAttributeValue("href", null);
                _logger.LogInformation($"Next page found: {nextPageUrl}.");
                return (nextPageUrl, true);
            }

            _logger.LogInformation("Next page not found.");
            return (null, false)!;
        }

        private string GetShortDescriptionText(HtmlDocument htmlDocument)
        {
            var descNode = htmlDocument.DocumentNode.SelectSingleNode(
                "//div[contains(@class,'woocommerce-product-details__short-description')]");

            if (descNode != null)
            {
                return HtmlEntity.DeEntitize(descNode.InnerText?.Trim() ?? string.Empty);
            }

            return string.Empty;
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
    }
