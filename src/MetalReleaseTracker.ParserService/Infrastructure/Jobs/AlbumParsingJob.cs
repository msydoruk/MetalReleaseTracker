using Hangfire;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;
using Newtonsoft.Json;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class AlbumParsingJob
{
    private readonly Func<DistributorCode, IParser> _parserResolver;
    private readonly IParsingSessionRepository _parsingSessionRepository;
    private readonly IAlbumParsedEventRepository _albumParsedEventRepository;
    private readonly ILogger<AlbumParsingJob> _logger;

    public AlbumParsingJob(
        Func<DistributorCode, IParser> parserResolver,
        IParsingSessionRepository parsingSessionRepository,
        IAlbumParsedEventRepository albumParsedEventRepository,
        ILogger<AlbumParsingJob> logger)
    {
        _parserResolver = parserResolver;
        _parsingSessionRepository = parsingSessionRepository;
        _albumParsedEventRepository = albumParsedEventRepository;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 0, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task RunParserJob(ParserDataSource parserDataSource, CancellationToken cancellationToken)
    {
        try
        {
            var parsingSession = await _parsingSessionRepository.GetIncompleteAsync(parserDataSource.DistributorCode, cancellationToken) ??
                                 await _parsingSessionRepository.AddAsync(parserDataSource.DistributorCode, parserDataSource.ParsingUrl, cancellationToken);

            var parser = _parserResolver(parserDataSource.DistributorCode);

            bool hasMorePages;
            do
            {
                _logger.LogInformation($"Parsing distributor: {parserDataSource.DistributorCode}, URL: {parsingSession.PageToProcess}");
                var pageParsedResult = await parser.ParseAsync(parsingSession.PageToProcess, cancellationToken);

                foreach (var albumParsedEvent in pageParsedResult.ParsedAlbums)
                {
                    await _albumParsedEventRepository.AddAsync(
                        parsingSession.Id,
                        JsonConvert.SerializeObject(albumParsedEvent),
                        cancellationToken);

                    _logger.LogInformation($"Added album parsed event to outbox storage. SKU: {albumParsedEvent.SKU}, Distributor: {albumParsedEvent.DistributorCode}.");
                }

                hasMorePages = pageParsedResult.HasMorePages;

                if (hasMorePages)
                {
                    await _parsingSessionRepository.UpdateNextPageToProcessAsync(parsingSession.Id, pageParsedResult.NextPageUrl!, cancellationToken);
                }
            }
            while (hasMorePages);

            await _parsingSessionRepository.UpdateParsingStatus(parsingSession.Id, AlbumParsingStatus.Parsed, cancellationToken);
            _logger.LogInformation($"Parsing completed for distributor: {parserDataSource.DistributorCode}");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error occurred while parsing distributor: {parserDataSource.DistributorCode}");
            throw;
        }
    }
}