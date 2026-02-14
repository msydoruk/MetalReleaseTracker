using TickerQ.Utilities.Base;

namespace MetalReleaseTracker.CatalogSyncService.Services.Jobs;

public class TickerQJobFunctions
{
    private readonly AlbumProcessingJob _albumProcessingJob;
    private readonly AlbumProcessedPublisherJob _albumProcessedPublisherJob;

    public TickerQJobFunctions(
        AlbumProcessingJob albumProcessingJob,
        AlbumProcessedPublisherJob albumProcessedPublisherJob)
    {
        _albumProcessingJob = albumProcessingJob;
        _albumProcessedPublisherJob = albumProcessedPublisherJob;
    }

    [TickerFunction("AlbumProcessingJob")]
    public async Task RunAlbumProcessingJob(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();
        await _albumProcessingJob.RunProcessingJob(cancellationToken);
    }

    [TickerFunction("AlbumProcessedPublisherJob")]
    public async Task RunAlbumProcessedPublisherJob(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();
        await _albumProcessedPublisherJob.RunPublisherJob(cancellationToken);
    }
}
