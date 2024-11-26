using HtmlAgilityPack;
using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Infrastructure.Exceptions;
using MetalReleaseTracker.Infrastructure.Parsers.Models;
using MetalReleaseTracker.Infrastructure.Utils;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Parsers
{
    public class OsmoseProductionsParser : IParser
    {
        private const int PageDelayMilliseconds = 1000;
        private readonly IHtmlLoader _htmlLoader;
        private readonly ILogger<OsmoseProductionsParser> _logger;

        public DistributorCode DistributorCode => DistributorCode.OsmoseProductions;

        public OsmoseProductionsParser(IHtmlLoader htmlLoader, ILogger<OsmoseProductionsParser> logger)
        {
            _htmlLoader = htmlLoader;
            _logger = logger;
        }

        public async Task<IEnumerable<AlbumDto>> ParseAlbums(string parsingUrl)
        {
            _logger.LogInformation($"Starting album parsing from URL: {parsingUrl}.");

            var albums = new List<AlbumDto>();
            string nextPageUrl = parsingUrl;
            bool hasMorePages;

            do
            {
                _logger.LogInformation($"Parsing albums from page: {nextPageUrl}.");
                var htmlDocument = await LoadAndValidateHtmlDocument(nextPageUrl);
                var parsedAlbums = ParseAlbumUrlsAndStatusesFromListPage(htmlDocument);

                foreach (var parsedAlbum in parsedAlbums)
                {
                    var albumDetails = await ParseAlbumDetails(parsedAlbum.Url);

                    if (albumDetails.IsSuccess)
                    {
                        albumDetails.Data.Status = parsedAlbum.Status;
                        albums.Add(albumDetails.Data);
                    }
                    else
                    {
                        _logger.LogError($"Failed to parse album: {albumDetails.ErrorMessage}");
                    }
                }

                (nextPageUrl, hasMorePages) = GetNextPageUrl(htmlDocument);

                await Task.Delay(PageDelayMilliseconds);
            }
            while (hasMorePages);

            _logger.LogInformation($"Completed album parsing from URL: {parsingUrl}. Total albums parsed: {albums.Count}.");
            return albums;
        }

        private async Task<ParsingResult<AlbumDto>> ParseAlbumDetails(string albumUrl)
        {
            _logger.LogInformation($"Parsing details for album URL: {albumUrl}.");
            var htmlDocument = await LoadAndValidateHtmlDocument(albumUrl);

            var bandName = ParseBandName(htmlDocument);
            var name = ParseAlbumName(htmlDocument);
            var sku = ParseSku(htmlDocument);

            if (!CheckRequiredFields(bandName, name, sku))
            {
                return CreateErrorParsingResult($"Missing band name or album name or SKU in the HTML document {albumUrl}. " +
                                                $"Band: {bandName}, Album: {name}, SKU: {sku}");
            }

            var media = ParseMediaType(htmlDocument);

            if (!media.HasValue || !Enum.IsDefined(typeof(MediaType), media))
            {
                return CreateErrorParsingResult($"Skipping album {name} due to unmatched media type: {media}.");
            }

            var releaseDate = ParseReleaseDate(htmlDocument);
            var price = ParsePrice(htmlDocument);
            var photoUrl = ParsePhotoUrl(htmlDocument);
            var label = ParseLabel(htmlDocument);
            var press = ParsePress(htmlDocument);
            var description = ParseDescription(htmlDocument);

            return new ParsingResult<AlbumDto>
            {
                Data = new AlbumDto
                {
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
                }
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
            return (null, false);
        }

        private string GetNodeValue(HtmlDocument document, string xPath)
        {
            var node = document.DocumentNode.SelectSingleNode(xPath);
            return node?.InnerText?.Trim();
        }

        private async Task<HtmlDocument> LoadAndValidateHtmlDocument(string url)
        {
            _logger.LogInformation($"Download an HTML document from a URL: {url}.");
            var htmlDocument = await _htmlLoader.LoadHtmlDocumentAsync(url);

            if (htmlDocument?.DocumentNode == null)
            {
                var error = $"Failed to load or parse the HTML document {url}.";
                _logger.LogError(error);
                throw new OsmoseProductionsParserException(error);
            }

            return htmlDocument;
        }

        private bool CheckRequiredFields(string bandName, string name, string sku)
        {
            if (string.IsNullOrEmpty(bandName) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(sku))
            {
                return false;
            }

            return true;
        }

        private ParsingResult<AlbumDto> CreateErrorParsingResult(string errorMessage)
        {
            return new ParsingResult<AlbumDto>
            {
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
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
            return !string.IsNullOrEmpty(releaseDateText) ? AlbumParser.ParseYear(releaseDateText.Split(':').Last().Trim()) : DateTime.MinValue;
        }

        private float ParsePrice(HtmlDocument htmlDocument)
        {
            var priceText = GetNodeValue(htmlDocument, "//span[@class='cufonCd ']");
            priceText = priceText?.Replace("&nbsp;", " ").Replace("EUR", " ").Trim();

            return AlbumParser.ParsePrice(priceText);
        }

        private string ParsePhotoUrl(HtmlDocument htmlDocument)
        {
            var photoNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='photo_prod_container']/a");

            return photoNode?.GetAttributeValue("access_url", null);
        }

        private MediaType? ParseMediaType(HtmlDocument htmlDocument)
        {
            var mediaTypeText = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Media:')]")?.Split(':').Last().Trim();
            var mediaType = mediaTypeText?.Split(' ').FirstOrDefault();

            return AlbumParser.ParseMediaType(mediaType);
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
            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Info :')]");

            if (descriptionNode == null)
            {
                return null;
            }

            var descriptionHtml = descriptionNode.InnerHtml;

            var descriptionText = descriptionHtml
                .Replace("Info :", " ")
                .Replace("<br>", "\n")
                .Replace("&nbsp;", " ")
                .Trim();

            return descriptionText;
        }

        private AlbumStatus? ParseStatus(HtmlNode node)
        {
            var statusNode = node.SelectSingleNode(".//span[@class='inforestock']");

            if (statusNode != null)
            {
                var statusText = statusNode.InnerText.Trim();

                return AlbumParser.ParseAlbumStatus(statusText);
            }

            return null;
        }

        private List<AlbumUrlAndStatus> ParseAlbumUrlsAndStatusesFromListPage(HtmlDocument htmlDocument)
        {
            var albumData = new List<AlbumUrlAndStatus>();

            var albumNodes = htmlDocument.DocumentNode.SelectNodes(".//div[@class='row GshopListingA']//div[@class='column three mobile-four']");

            if (albumNodes != null)
            {
                foreach (var node in albumNodes)
                {
                    var albumUrl = node.SelectSingleNode(".//a").GetAttributeValue("href", string.Empty).Trim();
                    var status = ParseStatus(node);

                    albumData.Add(new AlbumUrlAndStatus
                    {
                        Url = albumUrl,
                        Status = status
                    });
                }
            }

            return albumData;
        }
    }
}