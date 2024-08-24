﻿using System.Globalization;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Parsers;
using MetalReleaseTracker.Infrastructure.Loaders;

namespace MetalReleaseTracker.Infrastructure.Parsers
{
    public class OsmoseProductionsParser : IParser
    {
        private readonly HtmlLoader _htmlLoader;
        private readonly MediaTypeParser _mediaTypeParser;
        private readonly AlbumStatusParser _albumStatusParser;
        private readonly YearParser _yearParser;

        public OsmoseProductionsParser(HtmlLoader htmlLoader, MediaTypeParser mediaTypeParser, AlbumStatusParser albumStatusParser, YearParser yearParser)
        {
            _htmlLoader = htmlLoader;
            _mediaTypeParser = mediaTypeParser;
            _albumStatusParser = albumStatusParser;
            _yearParser = yearParser;
        }

        public async Task<IEnumerable<Album>> ParseAlbums(Distributor distributor)
        {
            var albums = new List<Album>();
            var baseUrl = distributor.ParsingUrl;
            string nextPageUrl = baseUrl;
            bool hasMorePages;

            do
            {
                var htmlDocument = await _htmlLoader.LoadHtmlDocumentAsync(nextPageUrl);

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

                (nextPageUrl, hasMorePages) = PaginationHelper.GetNextPageUrl(htmlDocument);

                await Task.Delay(1000);
            }
            while (hasMorePages);

            return albums;
        }

        private async Task<Album> ParseAlbumDetails(string albumUrl)
        {
            var htmlDocument = await _htmlLoader.LoadHtmlDocumentAsync(albumUrl);

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
            var releaseDate = releaseDateNode != null ? _yearParser.ParseYear(releaseDateNode.InnerText.Split(':').Last().Trim()) : DateTime.MinValue;

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
            var media = mediaNode != null ? _mediaTypeParser.ParseMediaType(mediaNode.InnerText.Split(':').Last().Trim()) : MediaType.Unknown;

            var labelNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Label :')]//a");
            var label = labelNode?.InnerText.Trim() ?? "Unknown Label";

            var pressNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Press :')]");
            var press = pressNode?.InnerText.Split(':').Last().Trim() ?? "Unknown Press";

            var descriptionNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'Info :')]");
            var description = descriptionNode?.InnerText.Trim() ?? "No Description";

            var statusNode = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='cufonEb' and contains(text(), 'New or Used :')]");
            var status = statusNode != null ? _albumStatusParser.ParseAlbumStatus(statusNode.InnerText.Split(':').Last().Trim()) : AlbumStatus.Unknown;

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
    }
}