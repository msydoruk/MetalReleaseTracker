using MetalReleaseTracker.ParserService.Domain.Interfaces;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class BandReferenceSyncJob
{
    private readonly IBandReferenceService _bandReferenceService;
    private readonly ILogger<BandReferenceSyncJob> _logger;

    public BandReferenceSyncJob(
        IBandReferenceService bandReferenceService,
        ILogger<BandReferenceSyncJob> logger)
    {
        _bandReferenceService = bandReferenceService;
        _logger = logger;
    }

    public async Task RunSyncJob(CancellationToken cancellationToken)
    {
        try
        {
            await _bandReferenceService.SyncUkrainianBandsAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred while syncing band references from Metal Archives.");
            throw;
        }
    }
}
