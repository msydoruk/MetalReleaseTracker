using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using TickerQ.Utilities.Base;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class TickerQJobFunctions
{
    private readonly BandReferenceSyncJob _bandReferenceSyncJob;
    private readonly CatalogueIndexJob _catalogueIndexJob;
    private readonly AlbumDetailParsingJob _albumDetailParsingJob;
    private readonly AlbumPublisherJob _albumPublisherJob;
    private readonly ImageThumbnailBackfillJob _imageThumbnailBackfillJob;

    public TickerQJobFunctions(
        BandReferenceSyncJob bandReferenceSyncJob,
        CatalogueIndexJob catalogueIndexJob,
        AlbumDetailParsingJob albumDetailParsingJob,
        AlbumPublisherJob albumPublisherJob,
        ImageThumbnailBackfillJob imageThumbnailBackfillJob)
    {
        _bandReferenceSyncJob = bandReferenceSyncJob;
        _catalogueIndexJob = catalogueIndexJob;
        _albumDetailParsingJob = albumDetailParsingJob;
        _albumPublisherJob = albumPublisherJob;
        _imageThumbnailBackfillJob = imageThumbnailBackfillJob;
    }

    [TickerFunction("BandReferenceSyncJob")]
    public async Task RunBandReferenceSyncJob(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();
        await _bandReferenceSyncJob.RunSyncJob(cancellationToken);
    }

    [TickerFunction("CatalogueIndexJob")]
    public async Task RunCatalogueIndexJob(
        TickerFunctionContext<ParserDataSource> context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();
        await _catalogueIndexJob.RunCatalogueIndexJob(context.Request, cancellationToken);
    }

    [TickerFunction("AlbumDetailParsingJob")]
    public async Task RunAlbumDetailParsingJob(
        TickerFunctionContext<ParserDataSource> context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();
        await _albumDetailParsingJob.RunDetailParsingJob(context.Request, cancellationToken);
    }

    [TickerFunction("AlbumPublisherJob")]
    public async Task RunAlbumPublisherJob(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();
        await _albumPublisherJob.RunPublisherJob(cancellationToken);
    }

    [TickerFunction("ImageThumbnailBackfillJob")]
    public async Task RunImageThumbnailBackfillJob(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();
        await _imageThumbnailBackfillJob.RunBackfillJob(cancellationToken);
    }
}
