using System.Text;
using MassTransit;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Events;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace MetalReleaseTracker.CatalogSyncService.Consumers;

public class AlbumParsedEventConsumer : IConsumer<AlbumParsedPublicationEvent>
{
    private readonly IParsingSessionWithRawAlbumsRepository _parsingSessionWithRawAlbumsRepository;
    private readonly IFileStorageService _fileStorageService;
    private ILogger<AlbumParsedEventConsumer> _logger;

    public AlbumParsedEventConsumer(
        IParsingSessionWithRawAlbumsRepository parsingSessionWithRawAlbumsRepository,
        IFileStorageService fileStorageService,
        ILogger<AlbumParsedEventConsumer> logger)
    {
        _parsingSessionWithRawAlbumsRepository = parsingSessionWithRawAlbumsRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AlbumParsedPublicationEvent> context)
    {
        try
        {
            var albumParsedPublicationEvent = context.Message;

            var rawAlbumJson = new StringBuilder();
            foreach (var storageFilePath in albumParsedPublicationEvent.StorageFilePaths)
            {
                rawAlbumJson.Append(await _fileStorageService.DownloadFileAsStringAsync(storageFilePath));
            }

            var rawAlbumEntities = JsonConvert.DeserializeObject<List<RawAlbumEntity>>(rawAlbumJson.ToString());

            var parsingSessionWithRawAlbumsEntity = new ParsingSessionWithRawAlbumsEntity
            {
                Id = albumParsedPublicationEvent.ParsingSessionId,
                DistributorCode = albumParsedPublicationEvent.DistributorCode,
                CreatedDate = albumParsedPublicationEvent.CreatedDate,
                RawAlbums = rawAlbumEntities
            };

            await _parsingSessionWithRawAlbumsRepository.AddAsync(parsingSessionWithRawAlbumsEntity);

            _logger.LogInformation($"Created new parsing session and added albums to raw collection. Distributor: {albumParsedPublicationEvent.DistributorCode}.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Error occurred while consuming parsing session.");
            throw;
        }
    }
}