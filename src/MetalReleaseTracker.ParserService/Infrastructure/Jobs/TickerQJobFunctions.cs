using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using TickerQ.Utilities.Base;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class TickerQJobFunctions
{
    private readonly BandReferenceSyncJob _bandReferenceSyncJob;
    private readonly CatalogueIndexJob _catalogueIndexJob;
    private readonly AlbumDetailParsingJob _albumDetailParsingJob;
    private readonly AlbumParsedPublisherJob _albumParsedPublisherJob;

    public TickerQJobFunctions(
        BandReferenceSyncJob bandReferenceSyncJob,
        CatalogueIndexJob catalogueIndexJob,
        AlbumDetailParsingJob albumDetailParsingJob,
        AlbumParsedPublisherJob albumParsedPublisherJob)
    {
        _bandReferenceSyncJob = bandReferenceSyncJob;
        _catalogueIndexJob = catalogueIndexJob;
        _albumDetailParsingJob = albumDetailParsingJob;
        _albumParsedPublisherJob = albumParsedPublisherJob;
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

    [TickerFunction("AlbumParsedPublisherJob")]
    public async Task RunAlbumParsedPublisherJob(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();
        await _albumParsedPublisherJob.RunPublisherJob(cancellationToken);
    }
}
