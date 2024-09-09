using System.Globalization;
using HtmlAgilityPack;
using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Enums;
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
                    throw new InvalidOperationException("Failed to load or parse the HTML document. Album");
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
                throw new InvalidOperationException("Failed to load or parse the HTML document. AlbumDetails");
            }

            var bandNameNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonAb']/a");

            if (bandNameNode == null)
            {
                throw new InvalidOperationException("Band name node is missing in the HTML document.");
            }

            var bandName = bandNameNode?.InnerText.Replace("&nbsp;", " ").Trim() ?? "Unknown Band";

            var skuNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Press :')]");
            var sku = skuNode?.InnerText.Split(':').Last().Trim() ?? "Unknown SKU";

            var nameNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='column twelve']//span[@class='cufonAb']");
            var name = nameNode?.InnerText.Replace("&nbsp;", " ").Trim() ?? "Unknown Album Name";

            if (!string.IsNullOrEmpty(bandName) && name.StartsWith(bandName + " "))
            {
                name = name.Substring(bandName.Length).Trim();
            }

            var releaseDateNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Year :')]");
            var releaseDate = releaseDateNode != null ? _albumParser.ParseYear(releaseDateNode.InnerText.Split(':').Last().Trim()) : DateTime.MinValue;

            var genreNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Genre :')]");
            var genre = genreNode?.InnerText.Trim() ?? "Unknown Genre";

            var priceNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonCd ']");
            var priceText = priceNode?.InnerText.Replace("&nbsp;", " ").Replace("EUR", " ").Trim();
            var price = priceText != null ? float.Parse(priceText, CultureInfo.InvariantCulture) : 0.0f;

            var purchaseUrlNode = htmlDocument.DocumentNode.SelectSingleNode("//a[@class='lienor']");
            var purchaseUrl = purchaseUrlNode?.GetAttributeValue("href", "purchaseUrl").Trim() ?? "Unknown URL";

            var photoUrlNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='column left four GshopListingALeft mobile-one']//img");
            var photoUrl = photoUrlNode?.GetAttributeValue("src", "img").Trim() ?? "Unknown Photo URL";

            var mediaNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Media:')]");
            var media = mediaNode != null ? _albumParser.ParseMediaType(mediaNode.InnerText.Split(':').Last().Trim()) : MediaType.Unknown;

            var labelNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Label :')]//a");
            var label = labelNode?.InnerText.Trim() ?? "Unknown Label";

            var pressNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Press :')]");
            var press = pressNode?.InnerText.Split(':').Last().Trim() ?? "Unknown Press";

            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Info :')]");
            var description = descriptionNode?.InnerText.Trim() ?? "No Description";

            var statusNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'New or Used :')]");
            var status = statusNode != null ? _albumParser.ParseAlbumStatus(statusNode.InnerText.Split(':').Last().Trim()) : AlbumStatus.Unknown;

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
    }
}