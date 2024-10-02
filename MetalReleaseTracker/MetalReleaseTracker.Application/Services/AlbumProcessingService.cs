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
                await ProcessAlbumsFromDistributor(distributor, distributor.ParsingUrl);
            }
        }

        public async Task<IEnumerable<AlbumDto>> ProcessAlbumsFromDistributor(Distributor distributor, string parsingUrl)
        {
            var parser = _parserFactory.CreateParser(distributor.Code);

            var parsedAlbums = await parser.ParseAlbums(parsingUrl);

            var bandCache = new Dictionary<string, Band>();
            var existingAlbums = await GetAlbumsFromDistributorById(distributor.Id);
            var processedAlbums = new List<AlbumDto>();

            foreach (var album in parsedAlbums)
            {
                var band = await GetOrAddBandToCache(bandCache, album.BandName);
                var existingAlbum = FindAlbumBySKU(existingAlbums, album.SKU);

                if (existingAlbum == null)
                {
                    var newAlbum = MapToAlbum(album, band);
                    await _albumService.AddAlbum(newAlbum);
                }
                else
                {
                    UpdateAlbumDetails(existingAlbum, album);
                    await _albumService.UpdateAlbum(existingAlbum);
                }

                processedAlbums.Add(album);
            }

            await HideAlbumsNotInParsedList(existingAlbums, parsedAlbums);

            return processedAlbums;
        }

        private async Task<IEnumerable<Album>> GetAlbumsFromDistributorById(Guid distributorId)
        {
            return await _albumService.GetAllAlbumsFromDistributor(distributorId);
        }

        private async Task<Band> GetOrAddBandToCache(Dictionary<string, Band> bandCache, string bandName)
        {
            if (bandCache.TryGetValue(bandName, out var cachedBand))
            {
                return cachedBand;
            }

            var band = await _bandService.GetBandByName(bandName);

            if (band != null)
            {
                bandCache[bandName] = band;
            }
            else
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

        private Album FindAlbumBySKU(IEnumerable<Album> albums, string sku)
        {
            return albums.FirstOrDefault(existingAlbum => existingAlbum.SKU == sku);
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
                IsHidden = false,
                ModificationTime = DateTime.UtcNow
            };
        }

        private void UpdateAlbumDetails(Album existingAlbum, AlbumDto albumDto)
        {
            existingAlbum.Price = albumDto.Price;
            existingAlbum.ModificationTime = DateTime.UtcNow;
        }

        private async Task HideAlbumsNotInParsedList(IEnumerable<Album> existingAlbums, IEnumerable<AlbumDto> parsedAlbums)
        {
            var parsedAlbumsSet = new HashSet<string>(parsedAlbums.Select(album => album.SKU));

            foreach (var existingAlbum in existingAlbums)
            {
                if (!parsedAlbumsSet.Contains(existingAlbum.SKU))
                {
                    existingAlbum.IsHidden = true;
                    existingAlbum.ModificationTime = DateTime.UtcNow;
                    await _albumService.UpdateAlbum(existingAlbum);
                }
            }
        }
    }
}
