using BenchmarkDotNet.Attributes;
using MassTransit;
using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MetalReleaseTracker.CatalogSyncService.Data.Events;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Implementation;
using MetalReleaseTracker.CatalogSyncService.Services.Jobs;
using MetalReleaseTracker.CatalogSyncService.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MetalReleaseTracker.Benchmarks.CatalogSyncService
{
    [MemoryDiagnoser]
    public class AlbumProcessedPublisherBenchmarks
    {
        private TestMongoDatabaseFixture _fixture;
        private Mock<ITopicProducer<AlbumProcessedPublicationEvent>> _topicProducerMock;
        private AlbumProcessedRepository _albumProcessedRepository;
        private Mock<ILogger<AlbumProcessedPublisherJob>> _loggerMock;
        private IOptions<AlbumProcessedPublisherJobSettings> _options;
        private AlbumProcessedPublisherJob _job;

        [Params(10, 100, 1000)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fixture = new TestMongoDatabaseFixture();
            _fixture.InitializeAsync().GetAwaiter().GetResult();
            
            _topicProducerMock = new Mock<ITopicProducer<AlbumProcessedPublicationEvent>>();
            _topicProducerMock
                .Setup(x => x.Produce(It.IsAny<AlbumProcessedPublicationEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _albumProcessedRepository = new AlbumProcessedRepository(_fixture.MongoDatabase, _fixture.MongoDbConfig);
            _loggerMock = new Mock<ILogger<AlbumProcessedPublisherJob>>();
            _options = Options.Create(new AlbumProcessedPublisherJobSettings { BatchSize = 50 });
            _job = new AlbumProcessedPublisherJob(_topicProducerMock.Object, _albumProcessedRepository, _loggerMock.Object, _options, null);
            
            ClearCollection().GetAwaiter().GetResult();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            ClearCollection().GetAwaiter().GetResult();
            for (int i = 0; i < DataSize; i++)
            {
                AlbumProcessedStatus status = (i % 4) switch
                {
                    0 => AlbumProcessedStatus.New,
                    1 => AlbumProcessedStatus.Updated,
                    2 => AlbumProcessedStatus.Deleted,
                    _ => AlbumProcessedStatus.Published
                };
                _albumProcessedRepository.AddAsync(new AlbumProcessedEntity
                {
                    SKU = $"SKU_{i}",
                    DistributorCode = DistributorCode.OsmoseProductions,
                    ProcessedStatus = status
                }, CancellationToken.None).GetAwaiter().GetResult();
            }
        }

        [Benchmark]
        public void PublishProcessedAlbums()
        {
            _job.RunPublisherJob(CancellationToken.None).GetAwaiter().GetResult();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            ClearCollection().GetAwaiter().GetResult();
            _fixture?.DisposeAsync().GetAwaiter().GetResult();
        }

        private async Task ClearCollection()
        {
            await _fixture.MongoDatabase.DropCollectionAsync(_fixture.MongoDbConfig.Value.ProcessedAlbumsCollectionName);
        }
    }
}