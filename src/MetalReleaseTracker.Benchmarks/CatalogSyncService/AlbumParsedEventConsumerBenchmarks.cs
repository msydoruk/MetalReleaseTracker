using BenchmarkDotNet.Attributes;
using MassTransit;
using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Consumers;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Events;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Implementation;
using MetalReleaseTracker.CatalogSyncService.Tests.Fixtures;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace MetalReleaseTracker.Benchmarks.CatalogSyncService
{
    [MemoryDiagnoser]
    public class AlbumParsedEventConsumerBenchmarks
    {
        private TestMongoDatabaseFixture _fixture;
        private ParsingSessionWithRawAlbumsRepository _sessionRepository;
        private Mock<IFileStorageService> _fileStorageMock;
        private Mock<ILogger<AlbumParsedEventConsumer>> _loggerMock;
        private AlbumParsedEventConsumer _consumer;

        [Params(10, 100, 1000)]
        public int DataSize { get; set; }

        private ConsumeContext<AlbumParsedPublicationEvent> _consumeContext;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fixture = new TestMongoDatabaseFixture();
            _fixture.InitializeAsync().GetAwaiter().GetResult();
            
            _sessionRepository = new ParsingSessionWithRawAlbumsRepository(_fixture.MongoDatabase, _fixture.MongoDbConfig);
            _fileStorageMock = new Mock<IFileStorageService>();
            _loggerMock = new Mock<ILogger<AlbumParsedEventConsumer>>();
            _consumer = new AlbumParsedEventConsumer(_sessionRepository, _fileStorageMock.Object, _loggerMock.Object);
            
            ClearCollection().GetAwaiter().GetResult();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            ClearCollection().GetAwaiter().GetResult();

            var parsingSessionId = Guid.NewGuid();
            var filePaths = new List<string>();
            
            filePaths.Add("bigFile.json");
            
            var rawAlbums = new List<RawAlbumEntity>();
            for (int i = 0; i < DataSize; i++)
            {
                rawAlbums.Add(new RawAlbumEntity
                {
                    SKU = $"SKU_{i}",
                    ParsingSessionId = parsingSessionId,
                });
            }
            
            var rawJson = JsonConvert.SerializeObject(rawAlbums);
            
            _fileStorageMock
                .Setup(x => x.DownloadFileAsStringAsync("bigFile.json", It.IsAny<CancellationToken>()))
                .ReturnsAsync(rawJson);

            var publicationEvent = new AlbumParsedPublicationEvent
            {
                ParsingSessionId = parsingSessionId,
                DistributorCode = DistributorCode.OsmoseProductions,
                CreatedDate = DateTime.UtcNow,
                StorageFilePaths = filePaths
            };

            var consumeContextMock = new Mock<ConsumeContext<AlbumParsedPublicationEvent>>();
            consumeContextMock.Setup(x => x.Message).Returns(publicationEvent);

            _consumeContext = consumeContextMock.Object;
        }

        [Benchmark]
        public void ConsumeParsedAlbums()
        {
            _consumer.Consume(_consumeContext).GetAwaiter().GetResult();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            ClearCollection().GetAwaiter().GetResult();
            _fixture.DisposeAsync().GetAwaiter().GetResult();
        }

        private async Task ClearCollection()
        {
            await _fixture.MongoDatabase.DropCollectionAsync(_fixture.MongoDbConfig.Value.ParsingSessionWithRawAlbumsCollectionName);
        }
    }
}