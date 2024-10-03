using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Application.Services
{
    public class AlbumProcessingService : IAlbumProcessingService
    {
        private readonly IParserFactory _parserFactory;
        private readonly IAlbumService _albumService;
        private readonly IBandService _bandService;
        private readonly IDistributorsService _distributorService;

        public AlbumProcessingService(IParserFactory parserFactory, IAlbumService albumService, IBandService bandService, IDistributorsService distributorService)
        {
            _parserFactory = parserFactory;
            _albumService = albumService;
            _bandService = bandService;
            _distributorService = distributorService;
        }

        public async Task SynchronizeAlbums()
        {
            var distributors = await _distributorService.GetAllDistributors();

            foreach (var distributor in distributors)
            {
                await ProcessAlbumsFromDistributor(distributor);
            }
        }

        public async Task ProcessAlbumsFromDistributor(Distributor distributor)
        {
            var parser = _parserFactory.CreateParser(distributor.Code);

            var parsedAlbums = await parser.ParseAlbums(distributor.ParsingUrl);

            var bandCache = new Dictionary<string, Band>();
            var existingAlbums = await _albumService.GetAlbumsByDistributor(distributor.Id);

            foreach (var album in parsedAlbums)
            {
                var band = await GetOrAddBandToCache(bandCache, album.BandName);
                var existingAlbum = existingAlbums.FirstOrDefault(existingAlbum => existingAlbum.SKU == album.SKU);

                if (existingAlbum == null)
                {
                    var newAlbum = MapToAlbum(album, band);
                    await _albumService.AddAlbum(newAlbum);
                }
                else
                {
                    UpdateAlbumPrice(existingAlbum, album);
                    await _albumService.UpdateAlbum(existingAlbum);
                }
            }

            await HideAlbumsNotInParsedList(existingAlbums, parsedAlbums);
        }

        private async Task<Band> GetOrAddBandToCache(Dictionary<string, Band> bandCache, string bandName)
        {
            if (!bandCache.TryGetValue(bandName, out var band))
            {
                band = await _bandService.GetBandByName(bandName) ?? CreateNewBand(bandName);

                UpdateBandCache(bandCache, bandName, band);

                if (band.Id == Guid.Empty)
                {
                    await _bandService.AddBand(band);
                }
            }

            return band;
        }

        private Band CreateNewBand(string bandName)
        {
            return new Band
            {
                Id = Guid.NewGuid(),
                Name = bandName
            };
        }

        private void UpdateBandCache(Dictionary<string, Band> bandCache, string bandName, Band band)
        {
            bandCache[bandName] = band;
        }

        private Album MapToAlbum(AlbumDto albumDto, Band band)
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
                Status = (AlbumStatus)albumDto.Status,
                ModificationTime = DateTime.UtcNow
            };
        }

        private void UpdateAlbumPrice(Album existingAlbum, AlbumDto albumDto)
        {
            existingAlbum.Price = albumDto.Price;
            existingAlbum.ModificationTime = DateTime.UtcNow;
        }

        private async Task HideAlbumsNotInParsedList(IEnumerable<Album> existingAlbums, IEnumerable<AlbumDto> parsedAlbums)
        {
            var parsedAlbumsSet = new HashSet<string>(parsedAlbums.Select(album => album.SKU));

            var albumsToUpdate = new List<Album>();

            foreach (var existingAlbum in existingAlbums)
            {
                if (!parsedAlbumsSet.Contains(existingAlbum.SKU))
                {
                    existingAlbum.Status = AlbumStatus.Unavailable;
                    existingAlbum.ModificationTime = DateTime.UtcNow;
                    albumsToUpdate.Add(existingAlbum);
                }
            }

            if (albumsToUpdate.Any())
            {
                await _albumService.UpdateAlbums(albumsToUpdate);
            }
        }
    }
}
