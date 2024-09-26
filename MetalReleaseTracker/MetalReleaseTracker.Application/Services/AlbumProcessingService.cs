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
                await ProcessAlbumsFromDistributor(distributor.Code, distributor.ParsingUrl);
            }
        }

        public async Task<IEnumerable<AlbumDto>> ProcessAlbumsFromDistributor(DistributorCode distributorCode, string parsingUrl)
        {
            var parser = _parserFactory.CreateParser(distributorCode);

            var parsedAlbums = await parser.ParseAlbums(parsingUrl);

            var bandCache = new Dictionary<string, Band>();
            var existingAlbums = GetAlbumsFromDistributor(distributorCode);
            var processedAlbums = new List<AlbumDto>();

            foreach (var album in parsedAlbums)
            {
                var band = await FindBandInCache(bandCache, album.BandName);
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

            await HideOldAlbums(existingAlbums, parsedAlbums);

            return processedAlbums;
        }

        private IEnumerable<Album> GetAlbumsFromDistributor(DistributorCode distributorCode)
        {
            var allAlbums = _albumService.GetAllAlbums().Result;
            return allAlbums.Where(album => album.Distributor.Code == distributorCode);
        }

        private async Task<Band> FindBandInCache(Dictionary<string, Band> bandCache, string bandName)
        {
            if (bandCache.TryGetValue(bandName, out var cachedBand))
            {
                return cachedBand;
            }

            var band = FindBandByName(bandCache.Values, bandName);

            if (band == null)
            {
                band = new Band
                {
                    Id = Guid.NewGuid(),
                    Name = bandName
                };

                bandCache[bandName] = band;

                await _bandService.AddBand(band);
            }

            return band;
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
                Status = (AlbumStatus)albumDto.Status,
                IsHidden = false,
                ModificationTime = DateTime.UtcNow
            };
        }

        private void UpdateExistingAlbum(Album existingAlbum, AlbumDto albumDto)
        {
            existingAlbum.Price = albumDto.Price;
            existingAlbum.ModificationTime = DateTime.UtcNow;
        }

        private async Task HideOldAlbums(IEnumerable<Album> existingAlbums, IEnumerable<AlbumDto> parsedAlbums) //переробити. додати статус на avalible. додати modificstiontime ishidded для альбома
        {
            foreach (var existingAlbum in existingAlbums)
            {
                if (!parsedAlbums.Any(parsedAlbum => parsedAlbum.SKU == existingAlbum.SKU))
                {
                    existingAlbum.IsHidden = true;
                    existingAlbum.ModificationTime = DateTime.UtcNow;
                    await _albumService.UpdateAlbum(existingAlbum);
                }
            }
        }
    }
}
