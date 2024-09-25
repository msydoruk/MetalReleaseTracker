using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Application.Services
{
    public class AlbumProcessingService
    {
        private readonly IParserFactory _parserFactory;
        private readonly IAlbumService _albumService;
        private readonly IBandService _bandService;

        public AlbumProcessingService(IParserFactory parserFactory, IAlbumService albumService, IBandService bandService)
        {
            _parserFactory = parserFactory;
            _albumService = albumService;
            _bandService = bandService;
        }

        public async Task<IEnumerable<AlbumDto>> ProcessAlbumsFromDistributor(DistributorCode distributorCode, string parsingUrl)
        {
            var parser = _parserFactory.CreateParser(distributorCode);

            var parsedAlbums = await parser.ParseAlbums(parsingUrl);

            var existingBands = await _bandService.GetAllBands();
            var existingAlbums = await _albumService.GetAllAlbums();
            var processedAlbums = new List<AlbumDto>();

            foreach (var album in parsedAlbums)
            {
                var band = FindBandByName(existingBands, album.BandName);
                var existingAlbum = FindAlbumBySKU(existingAlbums, album.SKU);

                if (existingAlbum == null)
                {
                    var newAlbum = CreateNewAlbum(album, band);
                    await _albumService.AddAlbum(newAlbum);
                }
                else
                {
                    UpdateExistingAlbum(existingAlbum, album);
                    await _albumService.UpdateAlbum(existingAlbum);
                }

                processedAlbums.Add(album);
            }

            await DeleteMatchingAlbumsBySKU(existingAlbums, parsedAlbums);

            return processedAlbums;
        }

        private Band FindBandByName(IEnumerable<Band> bands, string bandName)
        {
            return bands.FirstOrDefault(existingBand => existingBand.Name == bandName);
        }

        private Album FindAlbumBySKU(IEnumerable<Album> albums, string sku)
        {
            return albums.FirstOrDefault(existingAlbum => existingAlbum.SKU == sku);
        }

        private Album CreateNewAlbum(AlbumDto albumDto, Band band)
        {
            return new Album
            {
                Id = Guid.NewGuid(),
                BandId = band.Id,
                SKU = albumDto.SKU,
                Name = albumDto.Name,
                ReleaseDate = albumDto.ReleaseDate,
                Genre = albumDto.Genre,
                Price = albumDto.Price,
                PurchaseUrl = albumDto.PurchaseUrl,
                PhotoUrl = albumDto.PhotoUrl,
                Media = (MediaType)albumDto.Media,
                Label = albumDto.Label,
                Press = albumDto.Press,
                Description = albumDto.Description,
                Status = (AlbumStatus)albumDto.Status
            };
        }

        private void UpdateExistingAlbum(Album existingAlbum, AlbumDto albumDto)
        {
            existingAlbum.Name = albumDto.Name;
            existingAlbum.ReleaseDate = albumDto.ReleaseDate;
            existingAlbum.Genre = albumDto.Genre;
            existingAlbum.Price = albumDto.Price;
            existingAlbum.PurchaseUrl = albumDto.PurchaseUrl;
            existingAlbum.PhotoUrl = albumDto.PhotoUrl;
            existingAlbum.Media = (MediaType)albumDto.Media;
            existingAlbum.Label = albumDto.Label;
            existingAlbum.Press = albumDto.Press;
            existingAlbum.Description = albumDto.Description;
            existingAlbum.Status = (AlbumStatus)albumDto.Status;
        }

        private async Task DeleteMatchingAlbumsBySKU(IEnumerable<Album> existingAlbums, IEnumerable<AlbumDto> parsedAlbums)
        {
            foreach (var existingAlbum in existingAlbums)
            {
                if (parsedAlbums.Any(parsedAlbum => parsedAlbum.SKU == existingAlbum.SKU))
                {
                    await _albumService.DeleteAlbum(existingAlbum.Id);
                }
            }
        }
    }
}
