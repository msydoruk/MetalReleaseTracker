using System.Globalization;
using HtmlAgilityPack;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Infrastructure.Parsers
{
    public class OsmoseProductionsParser : IParser
    {
        private const string DefaultUrl = "https://www.osmoseproductions.com/liste/index.cfm?what=all&lng=2&tete=ukraine";

        private readonly HttpClient _httpClient;

        public OsmoseProductionsParser()
        {
            _httpClient = new HttpClient();
        }

        public async Task<IEnumerable<Album>> ParseAlbums(string url = DefaultUrl)
        {
            var albums = new List<Album>();
            var htmlDocument = await LoadHtmlDocumentAsync(url);

            var albumNodes = htmlDocument.DocumentNode.SelectNodes(".//div[@class='row GshopListingA']//div[@class='column three mobile-four']");

            if (albumNodes != null)
            {
                foreach (var node in albumNodes)
                {
                    var albumUrl = node.SelectSingleNode(".//a").GetAttributeValue("href", string.Empty).Trim();
                    var albumDetails = await ParseAlbumDetails(albumUrl);

                    var album = new Album
                    {
                        Id = Guid.NewGuid(),
                        DistributorId = albumDetails.DistributorId,
                        Distributor = albumDetails.Distributor,
                        BandId = albumDetails.BandId,
                        Band = albumDetails.Band,
                        SKU = albumDetails.SKU,
                        Name = albumDetails.Name,
                        ReleaseDate = albumDetails.ReleaseDate,
                        Genre = albumDetails.Genre,
                        Price = albumDetails.Price,
                        PurchaseUrl = albumDetails.PurchaseUrl,
                        PhotoUrl = albumDetails.PhotoUrl,
                        Media = albumDetails.Media,
                        Label = albumDetails.Label,
                        Press = albumDetails.Press,
                        Description = albumDetails.Description,
                        Status = albumDetails.Status
                    };

                    albums.Add(album);
                }
            }

            return albums;
        }

        public async Task<IEnumerable<Band>> ParseBands(string url = DefaultUrl)
        {
            var bands = new List<Band>();
            var htmlDocument = await LoadHtmlDocumentAsync(url);

            var bandNodes = htmlDocument.DocumentNode.SelectNodes(".//div[@class='row GshopListingA']//div[@class='column three mobile-four']");

            if (bandNodes != null)
            {
                foreach (var node in bandNodes)
                {
                    var band = new Band
                    {
                        Id = Guid.NewGuid(),
                        Name = node.SelectSingleNode(".//span[@class='TtypeC TcolorC']").InnerText.Trim()
                    };
                    bands.Add(band);
                }
            }

            return bands;
        }

        public async Task<Album> ParseAlbumDetails(string albumUrl)
        {
            var htmlDocument = await LoadHtmlDocumentAsync(albumUrl);

            var distributor = new Distributor
            {
                Id = Guid.NewGuid(),
                Name = "Osmose Productions"
            };

            var bandNameNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonAb']/a");
            var bandName = bandNameNode?.InnerText.Replace("&nbsp;", " ").Trim() ?? "Unknown Band";
            var band = new Band
            {
                Id = Guid.NewGuid(),
                Name = bandName
            };

            var skuNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Press :')]");
            var sku = skuNode?.InnerText.Split(':').Last().Trim() ?? "Unknown SKU";

            var nameNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='column twelve']//span[@class='cufonAb']");
            var name = nameNode?.InnerText.Replace("&nbsp;", " ").Trim() ?? "Unknown Album Name";

            if (!string.IsNullOrEmpty(bandName) && name.StartsWith(bandName + " "))
            {
                name = name.Substring(bandName.Length).Trim();
            }

            var releaseDateNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Year :')]");
            var releaseDate = releaseDateNode != null ? ParseYear(releaseDateNode.InnerText.Split(':').Last().Trim()) : DateTime.MinValue;

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
            var media = mediaNode != null ? ParseMediaType(mediaNode.InnerText.Split(':').Last().Trim()) : MediaType.Unknown;

            var labelNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Label :')]//a");
            var label = labelNode?.InnerText.Trim() ?? "Unknown Label";

            var pressNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Press :')]");
            var press = pressNode?.InnerText.Split(':').Last().Trim() ?? "Unknown Press";

            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Info :')]");
            var description = descriptionNode?.InnerText.Trim() ?? "No Description";

            var statusNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'New or Used :')]");
            var status = statusNode != null ? ParseAlbumStatus(statusNode.InnerText.Split(':').Last().Trim()) : AlbumStatus.Unknown;

            var album = new Album
            {
                Id = Guid.NewGuid(),
                DistributorId = distributor.Id,
                Distributor = distributor,
                BandId = band.Id,
                Band = band,
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

            return album;
        }

        public async Task<HtmlDocument> LoadHtmlDocumentAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var pageContents = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(pageContents);

            return htmlDocument;
        }

        private MediaType ParseMediaType(string mediaType)
        {
            return mediaType switch
            {
                "CD" => MediaType.CD,
                "LP" => MediaType.LP,
                "Tape" => MediaType.Tape,
                _ => MediaType.Unknown
            };
        }

        private AlbumStatus ParseAlbumStatus(string status)
        {
            return status switch
            {
                "New" => AlbumStatus.New,
                "Restock" => AlbumStatus.Restock,
                "Preorder" => AlbumStatus.Preorder,
                _ => AlbumStatus.Unknown
            };
        }

        private DateTime ParseYear(string year)
        {
            if (DateTime.TryParseExact(year?.Trim(), "yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                return date;
            }

            return DateTime.MinValue;
        }
    }
}
