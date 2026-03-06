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

            if (albumEvent.ProcessedStatus == AlbumProcessedStatus.Deleted)
            {
                await HandleDeleteAsync(albumEvent, distributorName);
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

            if (albumEvent.ProcessedStatus == AlbumProcessedStatus.New)
            {
                await _albumRepository.AddAsync(albumEntity);
                _logger.LogInformation($"Album {albumEvent.Name} was processed.");
            }
            else if (albumEvent.ProcessedStatus == AlbumProcessedStatus.Updated)
            {
                await _albumRepository.UpdateAsync(albumEntity);
                _logger.LogInformation($"Album {albumEvent.Name} was updated.");
            }

            await LogChangeAsync(albumEvent, distributorName);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error occurred while consuming processed albums.");
            throw;
        }
    }

    private async Task HandleDeleteAsync(AlbumProcessedPublicationEvent albumEvent, string distributorName)
    {
        var deleted = await _albumRepository.DeleteAsync(albumEvent.Id);

        if (!deleted && !string.IsNullOrEmpty(albumEvent.SKU))
        {
            _logger.LogWarning(
                "Album '{Name}' not found by Id {Id}, attempting SKU fallback: {SKU}.",
                albumEvent.Name,
                albumEvent.Id,
                albumEvent.SKU);

            deleted = await _albumRepository.DeleteBySkuAsync(albumEvent.SKU);
        }

        if (deleted)
        {
            _logger.LogInformation("Album '{Name}' was deleted.", albumEvent.Name);
        }
        else
        {
            _logger.LogWarning(
                "Album '{Name}' (Id={Id}, SKU={SKU}) not found for deletion — skipping.",
                albumEvent.Name,
                albumEvent.Id,
                albumEvent.SKU);
        }

        await LogChangeAsync(albumEvent, distributorName);
    }

    private async Task LogChangeAsync(AlbumProcessedPublicationEvent albumEvent, string distributorName)
    {
        var changeLogEntry = new AlbumChangeLogEntity
        {
            Id = Guid.NewGuid(),
            AlbumName = albumEvent.Name,
            BandName = albumEvent.BandName,
            DistributorName = distributorName,
            Price = albumEvent.Price,
            PurchaseUrl = albumEvent.ProcessedStatus == AlbumProcessedStatus.Deleted ? null : albumEvent.PurchaseUrl,
            ChangeType = albumEvent.ProcessedStatus.ToString(),
            ChangedAt = DateTime.UtcNow,
        };

        await _albumChangeLogRepository.AddAsync(changeLogEntry);
    }
}
