using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Application.Services
{
    public class AlbumSynchronizationService : IAlbumSynchronizationService
    {
        private readonly IParserFactory _parserFactory;
        private readonly IAlbumService _albumService;
        private readonly IBandService _bandService;
        private readonly IDistributorsService _distributorService;
        private readonly ILogger<AlbumSynchronizationService> _logger;

        public AlbumSynchronizationService(
            IParserFactory parserFactory,
            IAlbumService albumService,
            IBandService bandService,
            IDistributorsService distributorService,
            ILogger<AlbumSynchronizationService> logger)
        {
            _parserFactory = parserFactory;
            _albumService = albumService;
            _bandService = bandService;
            _distributorService = distributorService;
            _logger = logger;
        }

        public async Task SynchronizeAllAlbums()
        {
            _logger.LogInformation("Starting album synchronization for all distributors.");

            try
            {
                var distributors = await _distributorService.GetAllDistributors();

                foreach (var distributor in distributors)
                {
                    await SynchronizeAlbumsFromDistributor(distributor);
                }

                _logger.LogInformation("Album synchronization for all distributors is complete.");
            }
            catch (Exception exception)
            {
                 _logger.LogError(exception, "An error occurred during album synchronization.");
            }
        }

        private async Task SynchronizeAlbumsFromDistributor(Distributor distributor)
        {
            _logger.LogInformation($"Starting synchronization albums from distributor: {distributor.Name}.");

            try
            {
                var parser = _parserFactory.CreateParser(distributor.Code);

                var parsedAlbums = await parser.ParseAlbums(distributor.ParsingUrl);
                var existingAlbums = await _albumService.GetAlbumsByDistributor(distributor.Id);

                var bandCache = new Dictionary<string, Band>();
                var albumsWithPriceChanges = new Dictionary<Guid, float>();

                foreach (var parsedAlbum in parsedAlbums)
                {
                    var existingAlbum = existingAlbums.FirstOrDefault(existingAlbum => existingAlbum.SKU == parsedAlbum.SKU);

                    if (existingAlbum == null)
                    {
                        var band = await GetOrAddBand(bandCache, parsedAlbum.BandName);
                        var newAlbum = MapParsedAlbumToEntity(parsedAlbum);
                        newAlbum.DistributorId = distributor.Id;
                        newAlbum.BandId = band.Id;

                        await _albumService.AddAlbum(newAlbum);

                        _logger.LogInformation($"Added new album {parsedAlbum.Name} for band {parsedAlbum.BandName}.");
                    }
                    else
                    {
                        AddAlbumToPriceChangeList(existingAlbum, parsedAlbum, albumsWithPriceChanges);
                    }
                }

                await UpdateAlbumPrices(albumsWithPriceChanges);

                await MarkAlbumsAsUnavailable(existingAlbums, parsedAlbums);

                _logger.LogInformation($"Completed synchronization albums from distributor: {distributor.Name}.");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"An error occurred while synchronizing albums from distributor {distributor.Name}.");
            }
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

                    _logger.LogInformation($"Added new band: {bandName}.");
                }

                bandCache[bandName] = band;
            }

            return band;
        }

        private Album MapParsedAlbumToEntity(AlbumDto albumDto)
        {
            return new Album
            {
                Id = Guid.NewGuid(),
                SKU = albumDto.SKU,
                Name = albumDto.Name,
                ReleaseDate = DateTime.SpecifyKind(albumDto.ReleaseDate, DateTimeKind.Utc),
                Genre = albumDto.Genre,
                Price = albumDto.Price,
                PurchaseUrl = albumDto.PurchaseUrl,
                PhotoUrl = albumDto.PhotoUrl,
                Media = (MediaType)albumDto.Media,
                Label = albumDto.Label,
                Press = albumDto.Press,
                Description = albumDto.Description,
                Status = albumDto.Status != null ? (AlbumStatus)albumDto.Status : (AlbumStatus?)null,
                ModificationTime = DateTime.UtcNow
            };
        }

        private void AddAlbumToPriceChangeList(Album existingAlbum, AlbumDto parsedAlbum, Dictionary<Guid, float> albumsToUpdate)
        {
            if (existingAlbum.Price != parsedAlbum.Price)
            {
                albumsToUpdate.Add(existingAlbum.Id, parsedAlbum.Price);
                _logger.LogInformation($"Detected price change for album {existingAlbum.Name}. Added to the list for price update: old price = {existingAlbum.Price}, new price = {parsedAlbum.Price}.");
            }
        }

        private async Task UpdateAlbumPrices(Dictionary<Guid, float> albumsWithPriceChanges)
        {
            if (albumsWithPriceChanges.Any())
            {
                await _albumService.UpdateAlbumPrices(albumsWithPriceChanges);
                _logger.LogInformation($"Updated prices for {albumsWithPriceChanges.Count} albums.");
            }
        }

        private async Task MarkAlbumsAsUnavailable(IEnumerable<Album> existingAlbums, IEnumerable<AlbumDto> parsedAlbums)
        {
            var parsedAlbumsSet = new HashSet<string>(parsedAlbums.Select(album => album.SKU));

            var albumsToMarkUnavailable = new List<Guid>();

            foreach (var existingAlbum in existingAlbums)
            {
                if (!parsedAlbumsSet.Contains(existingAlbum.SKU))
                {
                    albumsToMarkUnavailable.Add(existingAlbum.Id);
                }
            }

            if (albumsToMarkUnavailable.Any())
            {
                await _albumService.UpdateAlbumsStatus(albumsToMarkUnavailable, AlbumStatus.Unavailable);
                _logger.LogInformation($"Marked {albumsToMarkUnavailable.Count} albums as unavailable.");
            }
        }
    }
}