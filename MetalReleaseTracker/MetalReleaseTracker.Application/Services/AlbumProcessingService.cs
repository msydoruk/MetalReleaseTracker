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

        public async Task SynchronizeAllAlbums()
        {
            var distributors = await _distributorService.GetAllDistributors();

            foreach (var distributor in distributors)
            {
                await SynchronizeAlbumsFromDistributor(distributor);
            }
        }

        private async Task SynchronizeAlbumsFromDistributor(Distributor distributor)
        {
            var parser = _parserFactory.CreateParser(distributor.Code);

            var parsedAlbums = await parser.ParseAlbums(distributor.ParsingUrl);

            var bandCache = new Dictionary<string, Band>();
            var existingAlbums = await _albumService.GetAlbumsByDistributor(distributor.Id);

            foreach (var album in parsedAlbums)
            {
                var band = await GetOrAddBand(bandCache, album.BandName);
                var existingAlbum = existingAlbums.FirstOrDefault(existingAlbum => existingAlbum.SKU == album.SKU);

                if (existingAlbum == null)
                {
                    var newAlbum = MapToAlbumFromDto(album, band);
                    await _albumService.AddAlbum(newAlbum);
                }
                else
                {
                    await UpdateAlbumPrice(existingAlbum, album);
                }
            }

            await MarkAlbumsAsUnavailable(existingAlbums, parsedAlbums);
        }

        private async Task<Band> GetOrAddBand(Dictionary<string, Band> bandCache, string bandName)
        {
            if (!bandCache.TryGetValue(bandName, out var band))
            {
                band = await _bandService.GetBandByName(bandName);

                if (band == null)
                {
                    band = new Band { Id = Guid.NewGuid(), Name = bandName };

                    await _bandService.AddBand(band);
                }

                bandCache[bandName] = band;
            }

            return band;
        }

        private Album MapToAlbumFromDto(AlbumDto albumDto, Band band)
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

        private async Task UpdateAlbumPrice(Album existingAlbum, AlbumDto albumDto)
        {
            var updatedAlbumPrices = new Dictionary<Guid, float>();

            if (existingAlbum.Price != albumDto.Price && albumDto.Price > 0)
            {
                existingAlbum.Price = albumDto.Price;
                existingAlbum.ModificationTime = DateTime.UtcNow;

                updatedAlbumPrices[existingAlbum.Id] = existingAlbum.Price;
            }

            if (updatedAlbumPrices.Any())
            {
                foreach (var albumPrice in updatedAlbumPrices)
                {
                    await _albumService.UpdatePriceForAlbums(new List<Guid> { albumPrice.Key }, albumPrice.Value);
                }
            }
        }

        private async Task MarkAlbumsAsUnavailable(IEnumerable<Album> existingAlbums, IEnumerable<AlbumDto> parsedAlbums)
        {
            var parsedAlbumsSet = new HashSet<string>(parsedAlbums.Select(album => album.SKU));

            var albumsToUpdate = new List<Guid>();

            foreach (var existingAlbum in existingAlbums)
            {
                if (!parsedAlbumsSet.Contains(existingAlbum.SKU))
                {
                    albumsToUpdate.Add(existingAlbum.Id);
                }
            }

            if (albumsToUpdate.Any())
            {
                await _albumService.UpdateAlbumsStatus(albumsToUpdate, AlbumStatus.Unavailable);
            }
        }
    }
}