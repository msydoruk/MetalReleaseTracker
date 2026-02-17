using BenchmarkDotNet.Attributes;
using MassTransit;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using MetalReleaseTracker.ParserService.Tests.Factories;
using MetalReleaseTracker.ParserService.Tests.Fixtures;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;

namespace MetalReleaseTracker.Benchmarks.ParserService
{
    [MemoryDiagnoser]
    public class AlbumPublisherJobBenchmarks
    {
        private TestPostgresDatabaseFixture _fixture;
        private ParsingSessionRepository _parsingSessionRepo;
        private AlbumParsedEventRepository _albumParsedEventRepo;
        private Mock<ILogger<AlbumParsedPublisherJob>> _loggerMock;
        private Mock<ITopicProducer<AlbumParsedPublicationEvent>> _topicProducerMock;
        private Mock<IFileStorageService> _fileStorageMock;
        private AlbumParsedPublisherJob _job;

        [Params(10, 100, 1000)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fixture = new TestPostgresDatabaseFixture();
            _fixture.InitializeAsync().GetAwaiter().GetResult();
            
            _parsingSessionRepo = new ParsingSessionRepository(_fixture.DbContext, new Mock<ILogger<ParsingSessionRepository>>().Object);
            _albumParsedEventRepo = new AlbumParsedEventRepository(_fixture.DbContext);
            _loggerMock = new Mock<ILogger<AlbumParsedPublisherJob>>();
            _topicProducerMock = MocksFactory.CreateTopicProducerMock();
            _fileStorageMock = MocksFactory.CreateFileStorageServiceMock();
            var settingsMock = MocksFactory.CreateAlbumParsedPublisherJobSettingsMock(500000);
            _job = new AlbumParsedPublisherJob(
                _albumParsedEventRepo,
                _parsingSessionRepo,
                _fileStorageMock.Object,
                _topicProducerMock.Object,
                _loggerMock.Object,
                settingsMock.Object
            );
            
            ClearDatabase().GetAwaiter().GetResult();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            ClearDatabase().GetAwaiter().GetResult();
            var session = _parsingSessionRepo.AddAsync(DistributorCode.OsmoseProductions, CancellationToken.None)
                .GetAwaiter().GetResult();
            for (int i = 0; i < DataSize; i++)
            {
                var payload = JsonConvert.SerializeObject(new AlbumParsedEvent
                {
                    SKU = $"SKU_{i}",
                    DistributorCode = DistributorCode.OsmoseProductions
                });
                _albumParsedEventRepo.AddAsync(session.Id, payload, CancellationToken.None).GetAwaiter().GetResult();
            }
            _parsingSessionRepo.UpdateParsingStatus(session.Id, AlbumParsingStatus.Parsed, CancellationToken.None).GetAwaiter().GetResult();
        }

        [Benchmark]
        public void PublishParsedAlbums()
        {
            _job.RunPublisherJob(CancellationToken.None).GetAwaiter().GetResult();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            ClearDatabase().GetAwaiter().GetResult();
            _fixture?.DisposeAsync().GetAwaiter().GetResult();
        }

        private async Task ClearDatabase()
        {
            await _fixture.DbContext.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"AlbumParsedEvents\", \"ParsingSessions\" RESTART IDENTITY CASCADE;"
            );
        }
    }
}