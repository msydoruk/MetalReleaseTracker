using AutoMapper;
using MassTransit;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Events;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Extensions;

namespace MetalReleaseTracker.CoreDataService.Consumers;

public class AlbumProcessedEventConsumer : IConsumer<AlbumProcessedPublicationEvent>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IBandRepository _bandRepository;
    private readonly IDistributorsRepository _distributorsRepository;
    private readonly IAlbumChangeLogRepository _albumChangeLogRepository;
    private readonly ILogger<AlbumProcessedEventConsumer> _logger;
    private readonly IMapper _mapper;

    public AlbumProcessedEventConsumer(
        IAlbumRepository albumRepository,
        IBandRepository bandRepository,
        IDistributorsRepository distributorsRepository,
        IAlbumChangeLogRepository albumChangeLogRepository,
        ILogger<AlbumProcessedEventConsumer> logger,
        IMapper mapper)
    {
        _albumRepository = albumRepository;
        _bandRepository = bandRepository;
        _distributorsRepository = distributorsRepository;
        _albumChangeLogRepository = albumChangeLogRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AlbumProcessedPublicationEvent> context)
    {
        try
        {
            var albumEvent = context.Message;
            string distributorName = albumEvent.DistributorCode.TryGetDisplayName();

            if (string.IsNullOrEmpty(albumEvent.SKU))
            {
                _logger.LogWarning(
                    "Album '{Name}' has no SKU — skipping.",
                    albumEvent.Name);
                return;
            }

            var existingAlbum = await _albumRepository.GetBySkuAsync(albumEvent.SKU);
            float? oldPrice = existingAlbum?.Price;

            if (albumEvent.ProcessedStatus == AlbumProcessedStatus.Deleted)
            {
                if (existingAlbum != null)
                {
                    await _albumRepository.DeleteAsync(existingAlbum.Id);
                    _logger.LogInformation("Album '{Name}' (SKU={SKU}) was deleted.", albumEvent.Name, albumEvent.SKU);
                }
                else
                {
                    _logger.LogWarning(
                        "Album '{Name}' (SKU={SKU}) not found for deletion — skipping.",
                        albumEvent.Name,
                        albumEvent.SKU);
                }

                await LogChangeAsync(albumEvent, distributorName, oldPrice);
                return;
            }

            if (string.IsNullOrEmpty(distributorName))
            {
                _logger.LogWarning(
                    $"Distributor name not mapped for code: {albumEvent.DistributorCode}.");
            }

            var bandId = await _bandRepository.GetOrAddAsync(albumEvent.BandName);
            var distributorId = await _distributorsRepository.GetOrAddAsync(distributorName);
            var albumEntity = _mapper.Map<AlbumProcessedPublicationEvent, AlbumEntity>(albumEvent);

            albumEntity.BandId = bandId;
            albumEntity.DistributorId = distributorId;

            if (existingAlbum != null)
            {
                albumEntity.Id = existingAlbum.Id;
                await _albumRepository.UpdateAsync(albumEntity);
                _logger.LogInformation("Album '{Name}' (SKU={SKU}) was updated.", albumEvent.Name, albumEvent.SKU);
            }
            else
            {
                albumEntity.Id = Guid.NewGuid();
                await _albumRepository.AddAsync(albumEntity);
                _logger.LogInformation("Album '{Name}' (SKU={SKU}) was added.", albumEvent.Name, albumEvent.SKU);
            }

            await LogChangeAsync(albumEvent, distributorName, oldPrice);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error occurred while consuming processed albums.");
            throw;
        }
    }

    private async Task LogChangeAsync(AlbumProcessedPublicationEvent albumEvent, string distributorName, float? oldPrice = null)
    {
        var changeLogEntry = new AlbumChangeLogEntity
        {
            Id = Guid.NewGuid(),
            AlbumName = albumEvent.Name,
            BandName = albumEvent.BandName,
            DistributorName = distributorName,
            Price = albumEvent.Price,
            OldPrice = oldPrice,
            PurchaseUrl = albumEvent.ProcessedStatus == AlbumProcessedStatus.Deleted ? null : albumEvent.PurchaseUrl,
            ChangeType = albumEvent.ProcessedStatus.ToString(),
            ChangedAt = DateTime.UtcNow,
        };

        await _albumChangeLogRepository.AddAsync(changeLogEntry);
    }
}
