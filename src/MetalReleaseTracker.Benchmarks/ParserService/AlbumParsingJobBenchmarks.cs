using BenchmarkDotNet.Attributes;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MetalReleaseTracker.Benchmarks.ParserService
{
    [MemoryDiagnoser]
    public class AlbumParsingJobBenchmarks
    {
        private TestPostgresDatabaseFixture _fixture;
        private ParsingSessionRepository _parsingSessionRepo;
        private AlbumParsedEventRepository _albumParsedEventRepo;
        private CatalogueIndexRepository _catalogueIndexRepo;
        private Mock<IAlbumDetailParser> _detailParserMock;
        private Mock<IImageUploadService> _imageUploadServiceMock;
        private Mock<ILogger<AlbumDetailParsingJob>> _loggerMock;
        private AlbumDetailParsingJob _job;

        [Params(10, 100, 1000)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fixture = new TestPostgresDatabaseFixture();
            _fixture.InitializeAsync().GetAwaiter().GetResult();

            _parsingSessionRepo = new ParsingSessionRepository(_fixture.DbContext, new Mock<ILogger<ParsingSessionRepository>>().Object);
            _albumParsedEventRepo = new AlbumParsedEventRepository(_fixture.DbContext);
            _catalogueIndexRepo = new CatalogueIndexRepository(_fixture.DbContext);
            _imageUploadServiceMock = new Mock<IImageUploadService>();
            _loggerMock = new Mock<ILogger<AlbumDetailParsingJob>>();
            _detailParserMock = new Mock<IAlbumDetailParser>();

            var generalParserSettingsMock = new Mock<IOptions<GeneralParserSettings>>();
            generalParserSettingsMock.Setup(x => x.Value).Returns(new GeneralParserSettings
            {
                MinDelayBetweenRequestsSeconds = 0,
                MaxDelayBetweenRequestsSeconds = 1
            });

            _job = new AlbumDetailParsingJob(
                _ => _detailParserMock.Object,
                _catalogueIndexRepo,
                _parsingSessionRepo,
                _albumParsedEventRepo,
                _imageUploadServiceMock.Object,
                generalParserSettingsMock.Object,
                _loggerMock.Object);

            ClearDatabase().GetAwaiter().GetResult();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            ClearDatabase().GetAwaiter().GetResult();

            var entries = new List<CatalogueIndexEntity>();
            for (int i = 0; i < DataSize; i++)
            {
                entries.Add(new CatalogueIndexEntity
                {
                    Id = Guid.NewGuid(),
                    DistributorCode = DistributorCode.OsmoseProductions,
                    BandName = $"Band_{i}",
                    AlbumTitle = $"Album_{i}",
                    RawTitle = $"Band_{i} - Album_{i}",
                    DetailUrl = $"https://test.com/album/{i}",
                    Status = CatalogueIndexStatus.Relevant,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            _fixture.DbContext.CatalogueIndex.AddRange(entries);
            _fixture.DbContext.SaveChanges();

            _detailParserMock
                .Setup(x => x.ParseAlbumDetailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AlbumParsedEvent
                {
                    SKU = "SKU_BENCH",
                    DistributorCode = DistributorCode.OsmoseProductions
                });
        }

        [Benchmark]
        public void RunDetailParsingJob()
        {
            _job.RunDetailParsingJob(CancellationToken.None).GetAwaiter().GetResult();
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
                "TRUNCATE TABLE \"AlbumParsedEvents\", \"ParsingSessions\", \"CatalogueIndex\", \"BandReferences\" RESTART IDENTITY CASCADE;"
            );
        }
    }
}
