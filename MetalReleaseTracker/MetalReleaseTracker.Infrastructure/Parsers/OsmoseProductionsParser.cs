using System.Globalization;
using HtmlAgilityPack;
using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Infrastructure.Loaders;

namespace MetalReleaseTracker.Infrastructure.Parsers
{
    public class OsmoseProductionsParser : IParser
    {
        private readonly HtmlLoader _htmlLoader;
        private readonly AlbumParser _albumParser;

        public DistributorCode DistributorCode => DistributorCode.OsmoseProductions;

        public OsmoseProductionsParser(HtmlLoader htmlLoader, AlbumParser albumParser)
        {
            _htmlLoader = htmlLoader;
            _albumParser = albumParser;
        }

        public async Task<IEnumerable<AlbumDto>> ParseAlbums(string parsingUrl)
        {
            var albums = new List<AlbumDto>();
            string nextPageUrl = parsingUrl;
            bool hasMorePages;

            do
            {
                var htmlDocument = await _htmlLoader.LoadHtmlDocumentAsync(nextPageUrl);

                if (htmlDocument == null || htmlDocument.DocumentNode == null)
                {
                    throw new OsmoseProductionsParserException("Failed to load or parse the HTML document", nextPageUrl);
                }

                var albumNodes = htmlDocument.DocumentNode.SelectNodes(".//div[@class='row GshopListingA']//div[@class='column three mobile-four']");

                if (albumNodes != null)
                {
                    foreach (var node in albumNodes)
                    {
                        var albumUrl = node.SelectSingleNode(".//a").GetAttributeValue("href", string.Empty).Trim();
                        var albumDetails = await ParseAlbumDetails(albumUrl);

                        albums.Add(albumDetails);
                    }
                }

                (nextPageUrl, hasMorePages) = GetNextPageUrl(htmlDocument);

                await Task.Delay(1000);
            }
            while (hasMorePages);

            return albums;
        }

        private async Task<AlbumDto> ParseAlbumDetails(string albumUrl)
        {
            var htmlDocument = await _htmlLoader.LoadHtmlDocumentAsync(albumUrl);

            if (htmlDocument == null || htmlDocument.DocumentNode == null)
            {
                throw new OsmoseProductionsParserException("Failed to load or parse the HTML document", albumUrl);
            }

            var bandName = GetNodeValue(htmlDocument, "//span[@class='cufonAb']/a") ?? "Unknown Band";
            var sku = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Press :')]")?.Split(':').Last().Trim() ?? "Unknown SKU";
            var name = GetNodeValue(htmlDocument, "//div[@class='column twelve']//span[@class='cufonAb']")?.Replace("&nbsp;", " ").Trim() ?? "Unknown Album Name";

            var nameParts = name.Split("</a>&nbsp;");
            if (nameParts.Length > 1)
            {
                name = nameParts[1].Trim();
            }
            else if (name.StartsWith(bandName + " "))
            {
                name = name.Substring(bandName.Length).Trim();
            }

            var releaseDateText = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Year :')]");
            var releaseDate = !string.IsNullOrEmpty(releaseDateText) ? _albumParser.ParseYear(releaseDateText.Split(':').Last().Trim()) : DateTime.MinValue;

            var genre = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Genre :')]");

            var priceText = GetNodeValue(htmlDocument, "//span[@class='cufonCd ']")?.Replace("&nbsp;", " ").Replace("EUR", " ").Trim();
            float price = 0.0f;
            if (!string.IsNullOrEmpty(priceText))
            {
                if (float.TryParse(priceText, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedPrice))
                {
                    price = parsedPrice;
                }
                else
                {
                    throw new OsmoseProductionsParserException($"Failed to parse price: {priceText}", albumUrl);
                }
            }

            var purchaseUrl = GetNodeAttribute(htmlDocument.DocumentNode, "//a[@class='lienor']", "href") ?? "Unknown URL";
            var photoUrl = GetNodeAttribute(htmlDocument.DocumentNode, "//div[@class='column left four GshopListingALeft mobile-one']//img", "src") ?? "Unknown Photo URL";

            var mediaTypeText = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Media:')]");
            var media = mediaTypeText != null ? _albumParser.ParseMediaType(mediaTypeText.Split(':').Last().Trim()) : (MediaType?)null;

            var label = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Label :')]//a");
            var press = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Press :')]")?.Split(':').Last().Trim();
            var description = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'Info :')]");

            var statusText = GetNodeValue(htmlDocument, "//span[@class='cufonEb' and contains(text(), 'New or Used :')]");
            var status = statusText != null ? _albumParser.ParseAlbumStatus(statusText.Split(':').Last().Trim()) : (AlbumStatus?)null;

            return new AlbumDto
            {
                BandName = bandName,
                SKU = sku,
                Name = name,
                ReleaseDate = releaseDate,
                Genre = genre,
                Price = price,
                PurchaseUrl = purchaseUrl,
                PhotoUrl = photoUrl,
                Media = media,
                Label = label,
                Press = press,
                Description = description,
                Status = status
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
    }
}