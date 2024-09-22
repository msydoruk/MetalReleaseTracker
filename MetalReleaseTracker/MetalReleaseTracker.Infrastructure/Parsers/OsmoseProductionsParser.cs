﻿using HtmlAgilityPack;
using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Infrastructure.Utils;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Parsers
{
    public class OsmoseProductionsParser : IParser
    {
        private readonly IHtmlLoader _htmlLoader;
        private readonly AlbumParser _albumParser;
        private readonly ILogger<OsmoseProductionsParser> _logger;

        public DistributorCode DistributorCode => DistributorCode.OsmoseProductions;

        public OsmoseProductionsParser(IHtmlLoader htmlLoader, AlbumParser albumParser, ILogger<OsmoseProductionsParser> logger)
        {
            _htmlLoader = htmlLoader;
            _albumParser = albumParser;
            _logger = logger;
        }

        public async Task<ParsingResultDto<List<AlbumDto>>> ParseAlbums(string parsingUrl)
        {
            var albums = new List<AlbumDto>();
            string nextPageUrl = parsingUrl;
            bool hasMorePages;
            var parsingResult = new ParsingResultDto<List<AlbumDto>> { Data = albums };

            do
            {
                var htmlDocument = await LoadAndValidateHtmlDocument(nextPageUrl);

                var albumNodes = htmlDocument.DocumentNode.SelectNodes(".//div[@class='row GshopListingA']//div[@class='column three mobile-four']");

                if (albumNodes != null)
                {
                    foreach (var node in albumNodes)
                    {
                        var albumUrl = node.SelectSingleNode(".//a").GetAttributeValue("href", string.Empty).Trim();
                        var albumDetails = await ParseAlbumDetails(albumUrl);

                        if (albumDetails.IsSuccess)
                        {
                            albums.Add(albumDetails.Data);
                        }
                        else
                        {
                            _logger.LogError($"Album {albumUrl} skipped. Reason: {albumDetails.ErrorMessage}");
                        }
                    }
                }

                (nextPageUrl, hasMorePages) = GetNextPageUrl(htmlDocument);

                await Task.Delay(1000);
            }
            while (hasMorePages);

            return parsingResult;
        }

        private async Task<ParsingResultDto<AlbumDto>> ParseAlbumDetails(string albumUrl)
        {
            var htmlDocument = await LoadAndValidateHtmlDocument(albumUrl);

            var bandName = ParseBandName(htmlDocument);
            var name = ParseAlbumName(htmlDocument);
            var sku = ParseSku(htmlDocument);

            if (!CheckRequiredFields(bandName, name, sku))
            {
                var message = $"Missing band name or album name or SKU in the HTML document {albumUrl}. " +
                  $"Band: {bandName ?? "Unknown"}, Album: {name ?? "Unknown"}, SKU: {sku ?? "Unknown"}";

                _logger.LogError(message);

                return new ParsingResultDto<AlbumDto>
                {
                    IsSuccess = false,
                    ErrorMessage = message
                };
            }

            var releaseDate = ParseReleaseDate(htmlDocument);
            var genre = ParseGenre(htmlDocument);
            var price = ParsePrice(htmlDocument);
            var photoUrl = ParsePhotoUrl(htmlDocument);
            var media = ParseMediaType(htmlDocument);
            var label = ParseLabel(htmlDocument);
            var press = ParsePress(htmlDocument);
            var description = ParseDescription(htmlDocument);
            var status = ParseStatus(htmlDocument);

            return new ParsingResultDto<AlbumDto>
            {
                Data = new AlbumDto
                {
                    BandName = bandName,
                    SKU = sku,
                    Name = name,
                    ReleaseDate = releaseDate,
                    Genre = genre,
                    Price = price,
                    PurchaseUrl = albumUrl,
                    PhotoUrl = photoUrl,
                    Media = media,
                    Label = label,
                    Press = press,
                    Description = description,
                    Status = status
                }
            };
        }

        private (string nextPageUrl, bool hasMorePages) GetNextPageUrl(HtmlDocument htmlDocument)
        {
            var nextPageNode = htmlDocument.DocumentNode.SelectSingleNode(".//div[@class='GtoursPagination']//a[contains(@href, 'page=') and not(contains(@href, 'javascript'))]");
            if (nextPageNode != null)
            {
                string nextPageUrl = nextPageNode.GetAttributeValue("href", null);
                if (!string.IsNullOrEmpty(nextPageUrl))
                {
                    return (nextPageUrl, true);
                }
            }

            return (null, false);
        }

        private string GetNodeValue(HtmlDocument document, string xPath)
        {
            var node = document.DocumentNode.SelectSingleNode(xPath);
            return node?.InnerText?.Trim();
        }

        private string GetNodeAttribute(HtmlNode node, string xPath, string attribute)
        {
            var selectedNode = node.SelectSingleNode(xPath);
            return selectedNode?.GetAttributeValue(attribute, null)?.Trim();
        }

        private async Task<HtmlDocument> LoadAndValidateHtmlDocument(string url)
        {
            var htmlDocument = await _htmlLoader.LoadHtmlDocumentAsync(url);

            if (htmlDocument?.DocumentNode == null)
            {
                throw new OsmoseProductionsParserException($"Failed to load or parse the HTML document {url}");
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
            return !string.IsNullOrEmpty(releaseDateText) ? _albumParser.ParseYear(releaseDateText.Split(':').Last().Trim()) : DateTime.MinValue;
        }

        private string ParseGenre(HtmlDocument htmlDocument)
        {
            return GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Genre :')]");
        }

        private float ParsePrice(HtmlDocument htmlDocument)
        {
            var priceText = GetNodeValue(htmlDocument, "//span[@class='cufonCd']");
            priceText = priceText?.Replace("&nbsp;", " ").Replace("EUR", " ").Trim();
            return _albumParser.ParsePrice(priceText);
        }

        private string ParsePhotoUrl(HtmlDocument htmlDocument)
        {
            return GetNodeAttribute(htmlDocument.DocumentNode, "//div[@class='column left four GshopListingALeft mobile-one']//img", "src");
        }

        private MediaType? ParseMediaType(HtmlDocument htmlDocument)
        {
            var mediaTypeText = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Media:')]")?.Split(':').Last().Trim();
            return _albumParser.ParseMediaType(mediaTypeText);
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
            return GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Info :')]");
        }

        private AlbumStatus? ParseStatus(HtmlDocument htmlDocument)
        {
            var statusText = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'New or Used :')]")?.Split(':').Last().Trim();

            return _albumParser.ParseAlbumStatus(statusText);
        }
    }
}