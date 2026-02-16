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

public class OsmoseProductionsParser : IListingParser, IAlbumDetailParser
    {
        private static readonly string[] CategoryUrls =
        [
            "https://www.osmoseproductions.com/liste/?what=label&tete=osmose&srt=2&fmt=11",
            "https://www.osmoseproductions.com/liste/?what=label&tete=osmose&srt=2&fmt=990001",
            "https://www.osmoseproductions.com/liste/?what=label&tete=osmose&srt=2&fmt=16"
        ];

        private readonly IHtmlDocumentLoader _htmlDocumentLoader;
        private readonly GeneralParserSettings _generalParserSettings;
        private readonly ILogger<OsmoseProductionsParser> _logger;
        private readonly Random _random = new();
        private Queue<string> _pendingCategoryUrls = new();
        private bool _categoryQueueInitialized;

        public DistributorCode DistributorCode => DistributorCode.OsmoseProductions;

        public OsmoseProductionsParser(
            IHtmlDocumentLoader htmlDocumentLoader,
            IOptions<GeneralParserSettings> generalParserSettings,
            ILogger<OsmoseProductionsParser> logger)
        {
            _htmlDocumentLoader = htmlDocumentLoader;
            _generalParserSettings = generalParserSettings.Value;
            _logger = logger;
        }

        public async Task<ListingPageResult> ParseListingsAsync(string url, CancellationToken cancellationToken)
        {
            var pageUrl = url;

            if (!_categoryQueueInitialized)
            {
                pageUrl = InitializeCategoryQueue(url);
                _categoryQueueInitialized = true;
            }

            _logger.LogInformation("Crawling OsmoseProductions page: {Url}.", pageUrl);

            var htmlDocument = await LoadHtmlDocument(pageUrl, cancellationToken);
            var listings = ParseAlbumListings(htmlDocument, cancellationToken);

            _logger.LogInformation("Parsed {Count} products from page.", listings.Count);

            var nextPageUrl = ResolveNextPageUrl(htmlDocument);

            return new ListingPageResult
            {
                Listings = listings,
                NextPageUrl = nextPageUrl
            };
        }

        public async Task<AlbumParsedEvent> ParseAlbumDetailAsync(string detailUrl, CancellationToken cancellationToken)
        {
            await DelayBetweenRequests(cancellationToken);
            return await ParseAlbumDetails(detailUrl, cancellationToken);
        }

        private List<ListingItem> ParseAlbumListings(HtmlDocument htmlDocument, CancellationToken cancellationToken)
        {
            var results = new List<ListingItem>();
            var albumNodes = htmlDocument.DocumentNode.SelectNodes(".//div[@class='GshopListingABorder']");

            if (albumNodes == null)
            {
                return results;
            }

            foreach (var albumNode in albumNodes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var anchorNode = albumNode.SelectSingleNode(".//div[contains(@class,'GshopListingARightInfo')]//a");
                if (anchorNode == null)
                {
                    continue;
                }

                var albumUrl = anchorNode.GetAttributeValue("href", string.Empty).Trim();
                if (string.IsNullOrEmpty(albumUrl))
                {
                    continue;
                }

                var bandName = anchorNode.SelectSingleNode(".//span[@class='TtypeC TcolorC']")?.InnerText?.Trim() ?? string.Empty;
                var albumTitle = anchorNode.SelectSingleNode(".//span[@class='TtypeH TcolorC']")?.InnerText?.Trim() ?? string.Empty;
                var rawTitle = string.IsNullOrEmpty(albumTitle) ? bandName : $"{bandName} - {albumTitle}";

                results.Add(new ListingItem(bandName, albumTitle, albumUrl, rawTitle));
            }

            return results;
        }

        private async Task<AlbumParsedEvent> ParseAlbumDetails(string albumUrl, CancellationToken cancellationToken)
        {
            var htmlDocument = await LoadHtmlDocument(albumUrl, cancellationToken);

            var bandName = ParseBandName(htmlDocument);
            var sku = ParseSku(htmlDocument);
            var name = ParseAlbumName(htmlDocument);
            var releaseDate = ParseReleaseDate(htmlDocument);
            var price = ParsePrice(htmlDocument);
            var photoUrl = ParsePhotoUrl(htmlDocument);
            var media = ParseMediaType(htmlDocument);
            var label = ParseLabel(htmlDocument);
            var press = ParsePress(htmlDocument);
            var description = ParseDescription(htmlDocument);
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
                PurchaseUrl = albumUrl,
                PhotoUrl = photoUrl,
                Media = media,
                Label = label,
                Press = press,
                Description = description,
                Status = status
            };
        }

        private string ExtractStatusFromDetailPage(HtmlDocument htmlDocument)
        {
            var infoElements = htmlDocument.DocumentNode.SelectNodes(".//*[contains(@class, 'info')]");

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

        private string InitializeCategoryQueue(string initialUrl)
        {
            var urls = new List<string>();

            foreach (var categoryUrl in CategoryUrls)
            {
                if (!string.Equals(categoryUrl, initialUrl, StringComparison.OrdinalIgnoreCase))
                {
                    urls.Add(categoryUrl);
                }
            }

            _pendingCategoryUrls = new Queue<string>(urls);

            _logger.LogInformation(
                "Initialized OsmoseProductions category queue. Starting with {Url}, {Remaining} categories remaining.",
                initialUrl,
                _pendingCategoryUrls.Count);

            return initialUrl;
        }

        private string? ResolveNextPageUrl(HtmlDocument htmlDocument)
        {
            var currentPageNode = htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='GtoursPaginationButtonTxt on']/span");

            if (currentPageNode != null && int.TryParse(currentPageNode.InnerText.Trim(), out int currentPageNumber))
            {
                var nextPageNode = htmlDocument.DocumentNode.SelectSingleNode($".//a[contains(@href, 'page={currentPageNumber + 1}')]");

                if (nextPageNode != null)
                {
                    var nextPageUrl = nextPageNode.GetAttributeValue("href", string.Empty).Trim();
                    if (!string.IsNullOrEmpty(nextPageUrl))
                    {
                        return nextPageUrl;
                    }
                }
            }

            if (_pendingCategoryUrls.Count > 0)
            {
                var nextCategory = _pendingCategoryUrls.Dequeue();
                _logger.LogInformation("Moving to next category: {Url}.", nextCategory);
                return nextCategory;
            }

            _logger.LogInformation("All OsmoseProductions categories crawled.");
            return null;
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
                throw new OsmoseProductionsParserException(error);
            }

            return htmlDocument;
        }

        private string ParseBandName(HtmlDocument htmlDocument)
        {
            var bandName = GetNodeValue(htmlDocument, "//span[@class='cufonAb']/a");

            return bandName;
        }

        private string ParseAlbumName(HtmlDocument htmlDocument)
        {
            var nameNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='column twelve']//span[@class='cufonAb']");
            var nameHtml = nameNode?.InnerHtml;
            var name = !string.IsNullOrEmpty(nameHtml)
                ? nameHtml.Split(new[] { "</a>&nbsp;" }, StringSplitOptions.None).Last().Trim()
                : null;

            return name;
        }

        private string ParseSku(HtmlDocument htmlDocument)
        {
            return GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Press :')]")?.Split(':').Last().Trim();
        }

        private DateTime ParseReleaseDate(HtmlDocument htmlDocument)
        {
            var releaseDateText = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Year :')]");
            return !string.IsNullOrEmpty(releaseDateText) ? AlbumParsingHelper.ParseYear(releaseDateText.Split(':').Last().Trim()) : DateTime.MinValue;
        }

        private float ParsePrice(HtmlDocument htmlDocument)
        {
            var priceText = GetNodeValue(htmlDocument, "//span[@class='cufonCd ']");
            priceText = priceText?.Replace("&nbsp;", " ").Replace("EUR", " ").Trim();

            return AlbumParsingHelper.ParsePrice(priceText);
        }

        private string ParsePhotoUrl(HtmlDocument htmlDocument)
        {
            var photoNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='photo_prod_container']/a");

            return photoNode?.GetAttributeValue("access_url", null);
        }

        private AlbumMediaType? ParseMediaType(HtmlDocument htmlDocument)
        {
            var mediaTypeText = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Media:')]")
                ?.Split(':').Last().Trim();

            return AlbumParsingHelper.ParseMediaType(mediaTypeText);
        }

        private string ParseLabel(HtmlDocument htmlDocument)
        {
            return GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Label :')]//a");
        }

        private string ParsePress(HtmlDocument htmlDocument)
        {
            return GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Press :')]")?.Split(':').Last().Trim();
        }

        private string ParseDescription(HtmlDocument htmlDocument)
        {
            var description = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Info :')]")?.InnerHtml;

            return description;
        }

        private async Task DelayBetweenRequests(CancellationToken cancellationToken)
        {
            var delaySeconds = _random.Next(_generalParserSettings.MinDelayBetweenRequestsSeconds,
                _generalParserSettings.MaxDelayBetweenRequestsSeconds);

            await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
        }
    }
