using BenchmarkDotNet.Attributes;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using MetalReleaseTracker.ParserService.Tests.Factories;
using MetalReleaseTracker.ParserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MetalReleaseTracker.Benchmarks.ParserService
{
    [MemoryDiagnoser]
    public class AlbumParsingJobBenchmarks
    {
        private TestPostgresDatabaseFixture _fixture;
        private ParsingSessionRepository _parsingSessionRepo;
        private AlbumParsedEventRepository _albumParsedEventRepo;
        private Mock<IParser> _parserMock;
        private Mock<IImageUploadService> _imageUploadServiceMock;
        private Mock<ILogger<AlbumParsingJob>> _loggerMock;
        private AlbumParsingJob _job;
        private ParserDataSource _dataSource;

        [Params(10, 100, 1000)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fixture = new TestPostgresDatabaseFixture();
            _fixture.InitializeAsync().GetAwaiter().GetResult();
            
            _parsingSessionRepo = new ParsingSessionRepository(_fixture.DbContext, new Mock<ILogger<ParsingSessionRepository>>().Object);
            _albumParsedEventRepo = new AlbumParsedEventRepository(_fixture.DbContext);
            _imageUploadServiceMock = new Mock<IImageUploadService>();
            _loggerMock = new Mock<ILogger<AlbumParsingJob>>();
            _parserMock = new Mock<IParser>();
            _job = new AlbumParsingJob(_ => _parserMock.Object, _parsingSessionRepo, _albumParsedEventRepo, _imageUploadServiceMock.Object, _loggerMock.Object);
            _dataSource = new ParserDataSource { DistributorCode = DistributorCode.OsmoseProductions, ParsingUrl = "https://test.com" };
            
            ClearDatabase().GetAwaiter().GetResult();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            ClearDatabase().GetAwaiter().GetResult();
            var fakeAlbums = new List<AlbumParsedEvent>();
            for (int i = 0; i < DataSize; i++)
            {
                fakeAlbums.Add(new AlbumParsedEvent { SKU = $"SKU_{i}", DistributorCode = DistributorCode.OsmoseProductions });
            }
            _parserMock = MocksFactory.CreateParserMock(fakeAlbums);
        }

        [Benchmark]
        public void RunParserJob()
        {
            _job.RunParserJob(_dataSource, CancellationToken.None).GetAwaiter().GetResult();
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