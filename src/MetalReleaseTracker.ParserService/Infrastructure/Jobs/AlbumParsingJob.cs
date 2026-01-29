using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Models;
using Newtonsoft.Json;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class AlbumParsingJob
{
    private readonly Func<DistributorCode, IParser> _parserResolver;
    private readonly IParsingSessionRepository _parsingSessionRepository;
    private readonly IAlbumParsedEventRepository _albumParsedEventRepository;
    //private readonly IImageUploadService _imageUploadService;
    private readonly ILogger<AlbumParsingJob> _logger;

    public AlbumParsingJob(
        Func<DistributorCode, IParser> parserResolver,
        IParsingSessionRepository parsingSessionRepository,
        IAlbumParsedEventRepository albumParsedEventRepository,
        //IImageUploadService imageUploadService,
        ILogger<AlbumParsingJob> logger)
    {
        _parserResolver = parserResolver;
        _parsingSessionRepository = parsingSessionRepository;
        _albumParsedEventRepository = albumParsedEventRepository;
        //_imageUploadService = imageUploadService;
        _logger = logger;
    }

    public async Task RunParserJob(ParserDataSource parserDataSource, CancellationToken cancellationToken)
    {
        try
        {
            await ExecuteParsingAsync(parserDataSource, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception,
                $"Error occurred while parsing distributor: {parserDataSource.DistributorCode}");
            throw;
        }
    }

    private async Task ExecuteParsingAsync(ParserDataSource parserDataSource, CancellationToken cancellationToken)
    {
        var parsingSession = await GetOrCreateParsingSessionAsync(parserDataSource, cancellationToken);
        var parser = _parserResolver(parserDataSource.DistributorCode);

        bool hasMorePages;
        do
        {
            _logger.LogInformation(
                $"Parsing distributor: {parserDataSource.DistributorCode}, URL: {parsingSession.PageToProcess}");
            var pageParsedResult = await parser.ParseAsync(parsingSession.PageToProcess, cancellationToken);

            await ProcessParsedAlbumsAsync(parsingSession.Id, pageParsedResult.ParsedAlbums, cancellationToken);

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

    private async Task<ParsingSessionEntity> GetOrCreateParsingSessionAsync(ParserDataSource parserDataSource, CancellationToken cancellationToken)
    {
        return await _parsingSessionRepository.GetIncompleteAsync(parserDataSource.DistributorCode,
                   cancellationToken) ??
               await _parsingSessionRepository.AddAsync(parserDataSource.DistributorCode, parserDataSource.ParsingUrl, cancellationToken);
    }

    private async Task ProcessParsedAlbumsAsync(Guid parsingSessionId, IEnumerable<AlbumParsedEvent> parsedAlbums, CancellationToken cancellationToken)
    {
        foreach (var albumParsedEvent in parsedAlbums)
        {
            await ProcessAlbumImageAsync(albumParsedEvent, cancellationToken);

            await _albumParsedEventRepository.AddAsync(
                parsingSessionId,
                JsonConvert.SerializeObject(albumParsedEvent),
                cancellationToken);

            _logger.LogInformation(
                $"Added album parsed event to outbox storage. SKU: {albumParsedEvent.SKU}, Distributor: {albumParsedEvent.DistributorCode}.");
        }
    }

    private async Task ProcessAlbumImageAsync(AlbumParsedEvent albumParsedEvent, CancellationToken cancellationToken)
    {
        var imageUploadRequest = new ImageUploadRequest
        {
            ImageUrl = albumParsedEvent.PhotoUrl,
            AlbumSku = albumParsedEvent.SKU ?? Guid.NewGuid().ToString(),
            DistributorCode = albumParsedEvent.DistributorCode,
            AlbumName = albumParsedEvent.Name,
            BandName = albumParsedEvent.BandName
        };

        //var uploadResult = await _imageUploadService.UploadAlbumImageAsync(imageUploadRequest, cancellationToken);

        //if (uploadResult.IsSuccess)
        //{
        //    albumParsedEvent.PhotoUrl = uploadResult.BlobPath;
        //}
    }
}