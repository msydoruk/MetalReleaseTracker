using MassTransit;
using MetalReleaseTracker.CoreDataService.Data.Events;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Consumers;

public class BandPhotoSyncedEventConsumer : IConsumer<BandPhotoSyncedEvent>
{
    private readonly IBandRepository _bandRepository;
    private readonly ILogger<BandPhotoSyncedEventConsumer> _logger;

    public BandPhotoSyncedEventConsumer(
        IBandRepository bandRepository,
        ILogger<BandPhotoSyncedEventConsumer> logger)
    {
        _bandRepository = bandRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<BandPhotoSyncedEvent> context)
    {
        var photoEvent = context.Message;

        try
        {
            var band = await _bandRepository.GetByNameAsync(photoEvent.BandName, context.CancellationToken);

            if (band == null)
            {
                _logger.LogWarning("Band '{BandName}' not found in CoreDataService — skipping photo update.", photoEvent.BandName);
                return;
            }

            band.PhotoUrl = photoEvent.PhotoBlobPath;

            if (string.IsNullOrEmpty(band.Genre) && !string.IsNullOrEmpty(photoEvent.Genre))
            {
                band.Genre = photoEvent.Genre;
            }

            await _bandRepository.UpdateAsync(band, context.CancellationToken);
            _logger.LogInformation("Updated photo for band '{BandName}' to '{PhotoUrl}'.", photoEvent.BandName, photoEvent.PhotoBlobPath);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error consuming BandPhotoSyncedEvent for band '{BandName}'.", photoEvent.BandName);
            throw;
        }
    }
}
