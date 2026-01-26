using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using TickerQ.Utilities.Base;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs.TickerQ;

public class TickerQJobFunctions
{
    private readonly AlbumParsingJob _albumParsingJob;
    private readonly AlbumParsedPublisherJob _albumParsedPublisherJob;

    public TickerQJobFunctions(
        AlbumParsingJob albumParsingJob,
        AlbumParsedPublisherJob albumParsedPublisherJob)
    {
        _albumParsingJob = albumParsingJob;
        _albumParsedPublisherJob = albumParsedPublisherJob;
    }

    [TickerFunction("AlbumParsingJob")]
    public async Task RunAlbumParsingJob(
        TickerFunctionContext<ParserDataSource> context,
        CancellationToken cancellationToken)
    {
        await _albumParsingJob.RunParserJob(context.Request, cancellationToken);
    }

    [TickerFunction("AlbumParsedPublisherJob")]
    public async Task RunAlbumParsedPublisherJob(
        TickerFunctionContext context,
        CancellationToken cancellationToken)
    {
        await _albumParsedPublisherJob.RunPublisherJob(cancellationToken);
    }
}
