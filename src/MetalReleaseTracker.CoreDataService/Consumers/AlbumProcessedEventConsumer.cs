using AutoMapper;
using MassTransit;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Events;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Extensions;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Utilities;

namespace MetalReleaseTracker.CoreDataService.Consumers;

public class AlbumProcessedEventConsumer : IConsumer<AlbumProcessedPublicationEvent>
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IBandRepository _bandRepository;
    private readonly IDistributorsRepository _distributorsRepository;
    private readonly IAlbumChangeLogRepository _albumChangeLogRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<AlbumProcessedEventConsumer> _logger;
    private readonly IMapper _mapper;

    public AlbumProcessedEventConsumer(
        IAlbumRepository albumRepository,
        IBandRepository bandRepository,
        IDistributorsRepository distributorsRepository,
        IAlbumChangeLogRepository albumChangeLogRepository,
        INotificationService notificationService,
        ILogger<AlbumProcessedEventConsumer> logger,
        IMapper mapper)
    {
        _albumRepository = albumRepository;
        _bandRepository = bandRepository;
        _distributorsRepository = distributorsRepository;
        _albumChangeLogRepository = albumChangeLogRepository;
        _notificationService = notificationService;
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
            AlbumStockStatus? oldStockStatus = existingAlbum?.StockStatus;

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

                await LogChangeAsync(albumEvent, distributorName, existingAlbum?.Slug ?? string.Empty, oldPrice, oldStockStatus);
                return;
            }

            if (string.IsNullOrEmpty(distributorName))
            {
                _logger.LogWarning(
                    $"Distributor name not mapped for code: {albumEvent.DistributorCode}.");
            }

            var bandId = await _bandRepository.GetOrAddAsync(albumEvent.BandName);

            if (!string.IsNullOrWhiteSpace(albumEvent.MetalArchivesUrl))
            {
                var bandEntity = await _bandRepository.GetByIdAsync(bandId);
                if (bandEntity != null && string.IsNullOrWhiteSpace(bandEntity.MetalArchivesUrl))
                {
                    bandEntity.MetalArchivesUrl = albumEvent.MetalArchivesUrl;
                    await _bandRepository.UpdateAsync(bandEntity);
                }
            }

            var distributorId = await _distributorsRepository.GetOrAddAsync(distributorName);
            var albumEntity = _mapper.Map<AlbumProcessedPublicationEvent, AlbumEntity>(albumEvent);

            albumEntity.BandId = bandId;
            albumEntity.DistributorId = distributorId;

            if (existingAlbum != null)
            {
                albumEntity.Id = existingAlbum.Id;
                albumEntity.Slug = existingAlbum.Slug;
                await _albumRepository.UpdateAsync(albumEntity);
                _logger.LogInformation("Album '{Name}' (SKU={SKU}) was updated.", albumEvent.Name, albumEvent.SKU);
            }
            else
            {
                albumEntity.Id = Guid.NewGuid();
                albumEntity.Slug = await GenerateUniqueAlbumSlugAsync(
                    albumEvent.BandName, albumEvent.CanonicalTitle ?? albumEvent.Name);
                await _albumRepository.AddAsync(albumEntity);
                _logger.LogInformation("Album '{Name}' (SKU={SKU}) was added.", albumEvent.Name, albumEvent.SKU);
            }

            await _notificationService.GenerateNotificationsAsync(albumEvent, existingAlbum, bandId, context.CancellationToken);

            bool hasPriceChange = oldPrice.HasValue && Math.Abs(oldPrice.Value - albumEvent.Price) > 0.001f;
            bool hasStatusChange = oldStockStatus.HasValue && oldStockStatus != albumEvent.StockStatus;
            bool isNewAlbum = albumEvent.ProcessedStatus != AlbumProcessedStatus.Updated;

            if (isNewAlbum || hasPriceChange || hasStatusChange)
            {
                await LogChangeAsync(albumEvent, distributorName, albumEntity.Slug, oldPrice, oldStockStatus);
            }
            else
            {
                _logger.LogDebug(
                    "Skipping changelog for '{AlbumName}' (SKU={SKU}) — no actual change in CoreDataService.",
                    albumEvent.Name,
                    albumEvent.SKU);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error occurred while consuming processed albums.");
            throw;
        }
    }

    private async Task<string> GenerateUniqueAlbumSlugAsync(string bandName, string albumName)
    {
        var slug = SlugGenerator.GenerateSlug(bandName, albumName);

        if (string.IsNullOrEmpty(slug))
        {
            slug = "unnamed";
        }

        var existing = await _albumRepository.GetBySlugAsync(slug);
        if (existing == null)
        {
            return slug;
        }

        var suffix = 2;
        while (await _albumRepository.GetBySlugAsync($"{slug}-{suffix}") != null)
        {
            suffix++;
        }

        return $"{slug}-{suffix}";
    }

    private async Task LogChangeAsync(
        AlbumProcessedPublicationEvent albumEvent,
        string distributorName,
        string albumSlug,
        float? oldPrice = null,
        AlbumStockStatus? oldStockStatus = null)
    {
        var changeLogEntry = new AlbumChangeLogEntity
        {
            Id = Guid.NewGuid(),
            AlbumName = albumEvent.Name,
            BandName = albumEvent.BandName,
            DistributorName = distributorName,
            Price = albumEvent.Price,
            OldPrice = oldPrice,
            StockStatus = albumEvent.StockStatus?.ToString(),
            OldStockStatus = oldStockStatus?.ToString(),
            PurchaseUrl = albumEvent.ProcessedStatus == AlbumProcessedStatus.Deleted ? null : albumEvent.PurchaseUrl,
            AlbumSlug = albumSlug,
            ChangeType = albumEvent.ProcessedStatus.ToString(),
            ChangeReason = albumEvent.ChangeReason,
            ChangedAt = DateTime.UtcNow,
        };

        await _albumChangeLogRepository.AddAsync(changeLogEntry);
    }
}
