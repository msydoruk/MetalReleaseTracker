using System.Text;
using Hangfire;
using MassTransit;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Messaging.Extensions;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class AlbumParsedPublisherJob
{
    private readonly IAlbumParsedEventRepository _albumParsedEventRepository;
    private readonly IParsingSessionRepository _parsingSessionRepository;
    private readonly ITopicProducer<AlbumParsedPublicationEvent> _topicProducer;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<AlbumParsedPublisherJob> _logger;
    private readonly AlbumParsedPublisherJobSettings _albumParsedPublisherSettings;

    public AlbumParsedPublisherJob(
        IAlbumParsedEventRepository albumParsedEventRepository,
        IParsingSessionRepository parsingSessionRepository,
        IFileStorageService fileStorageService,
        ITopicProducer<AlbumParsedPublicationEvent> topicProducer,
        ILogger<AlbumParsedPublisherJob> logger,
        IOptions<AlbumParsedPublisherJobSettings> options)
    {
        _parsingSessionRepository = parsingSessionRepository;
        _albumParsedEventRepository = albumParsedEventRepository;
        _fileStorageService = fileStorageService;
        _topicProducer = topicProducer;
        _logger = logger;
        _albumParsedPublisherSettings = options.Value;
    }

    [AutomaticRetry(Attempts = 10, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    public async Task RunPublisherJob(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var parsingSessions = await _parsingSessionRepository.GetParsedAsync(cancellationToken);

                if (!parsingSessions.Any())
                {
                    break;
                }

                foreach (var parsingSession in parsingSessions)
                {
                    var albumsToPublish = await _albumParsedEventRepository.GeAsync(parsingSession.Id, cancellationToken);

                    try
                    {
                        var storageFilePaths = await UploadToFileStorageAsync(parsingSession.DistributorCode, albumsToPublish, cancellationToken);
                        await PublishEvent(parsingSession, storageFilePaths, cancellationToken);
                        await _parsingSessionRepository.UpdateParsingStatus(parsingSession.Id, AlbumParsingStatus.Published, cancellationToken);
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, $"Albums publishing failed. Parsing state: {parsingSession.Id}.");
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error occurred while publishing albums to parser service topic.");
            throw;
        }
    }

    private async Task<List<string>> UploadToFileStorageAsync(
        DistributorCode distributorCode,
        List<AlbumParsedEventEntity> albumsToPublish,
        CancellationToken cancellationToken)
    {
        var albumObjects = albumsToPublish.Select(album => JsonConvert.DeserializeObject<AlbumParsedEvent>(album.EventPayload));
        var serializedAlbums = JsonConvert.SerializeObject(albumObjects, Formatting.Indented);
        var albumsBytes = Encoding.UTF8.GetBytes(serializedAlbums);

        var chunks = albumsBytes.SplitIntoChunks(_albumParsedPublisherSettings.MaxChunkSizeInBytes).ToList();

        var filePaths = new List<string>();
        for (int i = 0; i < chunks.Count; i++)
        {
            var filePath = $"{Guid.NewGuid()}/{distributorCode}_chunk{i + 1}.json";
            using var memoryStream = new MemoryStream(chunks[i]);

            await _fileStorageService.UploadFileAsync(filePath, memoryStream, cancellationToken);

            filePaths.Add(filePath);
        }

        return filePaths;
    }

    private async Task PublishEvent(ParsingSessionEntity parsingSessionEntity, List<string> storageFilePaths, CancellationToken cancellationToken)
    {
        var albumParsedPublicationEvent = new AlbumParsedPublicationEvent
        {
            CreatedDate = DateTime.UtcNow,
            ParsingSessionId = parsingSessionEntity.Id,
            DistributorCode = parsingSessionEntity.DistributorCode,
            StorageFilePaths = storageFilePaths
        };

        await _topicProducer.Produce(albumParsedPublicationEvent, cancellationToken);

        _logger.LogInformation($"Published albums to parser service topic. Distributor: {albumParsedPublicationEvent.DistributorCode}.");
    }
}