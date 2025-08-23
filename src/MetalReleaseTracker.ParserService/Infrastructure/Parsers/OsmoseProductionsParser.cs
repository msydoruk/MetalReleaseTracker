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

public class OsmoseProductionsParser : IParser
    {
        private readonly IHtmlDocumentLoader _htmlDocumentLoader;
        private readonly GeneralParserSettings _generalParserSettings;
        private readonly ILogger<OsmoseProductionsParser> _logger;
        private readonly Random _random = new();

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

        public async Task<PageParsedResult> ParseAsync(string parsingUrl, CancellationToken cancellationToken)
        {
            var nextPageUrl = parsingUrl;
            var htmlDocument = await LoadHtmlDocument(nextPageUrl, cancellationToken);
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

            (nextPageUrl, bool hasMorePages) = GetNextPageUrl(htmlDocument);

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
            var albumNodes = htmlDocument.DocumentNode.SelectNodes(".//div[@class='GshopListingABorder']");

            if (albumNodes != null)
            {
                foreach (var albumNode in albumNodes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var anchorNode = albumNode.SelectSingleNode(".//a");
                    if (anchorNode == null) continue;

                    var albumUrl = anchorNode.GetAttributeValue("href", string.Empty).Trim();
                    if (string.IsNullOrEmpty(albumUrl)) continue;

                    var statusText = ExtractStatusText(albumNode);
                    var status = string.IsNullOrEmpty(statusText)
                        ? null
                        : AlbumParsingHelper.ParseAlbumStatus(statusText);

                    results.Add((albumUrl, status));
                }
            }

            return results;
        }

        private string ExtractStatusText(HtmlNode albumNode)
        {
            var infoElements = albumNode.SelectNodes(".//*[contains(@class, 'info')]");

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

        private (string nextPageUrl, bool hasMorePages) GetNextPageUrl(HtmlDocument htmlDocument)
        {
            var currentPageNode = htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='GtoursPaginationButtonTxt on']/span");

            if (currentPageNode != null && int.TryParse(currentPageNode.InnerText.Trim(), out int currentPageNumber))
            {
                var nextPageNode = htmlDocument.DocumentNode.SelectSingleNode($".//a[contains(@href, 'page={currentPageNumber + 1}')]");

                if (nextPageNode != null)
                {
                    string nextPageUrl = nextPageNode.GetAttributeValue("href", null);
                    _logger.LogInformation($"Next page found: {nextPageUrl}.");

                    return (nextPageUrl, true);
                }
            }

            _logger.LogInformation("Next page not found.");
            return (null, false)!;
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