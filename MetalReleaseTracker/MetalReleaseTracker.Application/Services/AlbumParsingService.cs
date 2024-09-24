using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Application.Services
{
    public class AlbumParsingService
    {
        private readonly IParserFactory _parserFactory;
        private readonly IAlbumService _albumService;
        private readonly IBandService _bandService;

        public AlbumParsingService(IParserFactory parserFactory, IAlbumService albumService, IBandService bandService)
        {
            _parserFactory = parserFactory;
            _albumService = albumService;
            _bandService = bandService;
        }

        public async Task<IEnumerable<AlbumDto>> GetAlbumsFromDistributor(DistributorCode distributorCode, string parsingUrl)
        {
            var parser = _parserFactory.CreateParser(distributorCode);

            var albums = await parser.ParseAlbums(parsingUrl);

            var existingBands = await _bandService.GetAllBands();
            var existingAlbums = await _albumService.GetAllAlbums();
            var processedAlbums = new List<AlbumDto>();

            foreach (var album in albums)
            {
                var band = existingBands.FirstOrDefault(existingBand => existingBand.Name == album.BandName);

                if (!existingAlbums.Any(existingAlbum => existingAlbum.Name == album.Name))
                {
                    var newAlbum = new Album
                    {
                        Id = Guid.NewGuid(),
                        BandId = band.Id,
                        SKU = album.SKU,
                        Name = album.Name,
                        ReleaseDate = album.ReleaseDate,
                        Genre = album.Genre,
                        Price = album.Price,
                        PurchaseUrl = album.PurchaseUrl,
                        PhotoUrl = album.PhotoUrl,
                        Media = (MediaType)album.Media,
                        Label = album.Label,
                        Press = album.Press,
                        Description = album.Description,
                        Status = (AlbumStatus)album.Status
                    };
                    await _albumService.AddAlbum(newAlbum);

                    processedAlbums.Add(album);
                }
            }

            return processedAlbums;
        }
    }
}
