using System.Text.Json;
using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using MetalReleaseTracker.ParserService.Tests.Factories;
using MetalReleaseTracker.ParserService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.IntegrationTests;

public class AlbumParsingJobTests : IClassFixture<TestPostgresDatabaseFixture>, IAsyncLifetime
{
    private readonly TestPostgresDatabaseFixture _fixture;
    private readonly ParsingSessionRepository _parsingSessionRepository;
    private readonly AlbumParsedEventRepository _albumParsedEventRepository;
    private readonly Mock<ILogger<AlbumParsingJob>> _albumParsingJobLoggerMock = new();
    private readonly Mock<ILogger<ParsingSessionRepository>> _parsingSessionRepositoryLoggerMock = new();
    private readonly Mock<IImageUploadService> _imageUploadServiceMock = new();
    private readonly AlbumParsingJob _albumParsingJob;
    private readonly ParserDataSource _parserDataSource;
    private Mock<IParser> _parserMock = new();

    public AlbumParsingJobTests(TestPostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
        _parsingSessionRepository = new ParsingSessionRepository(_fixture.DbContext, _parsingSessionRepositoryLoggerMock.Object);
        _albumParsedEventRepository = new AlbumParsedEventRepository(_fixture.DbContext);
        _parserDataSource = new ParserDataSource
        {
            DistributorCode = DistributorCode.OsmoseProductions,
            ParsingUrl = "https://www.test.com"
        };

        _albumParsingJob = new AlbumParsingJob(
            _ => _parserMock.Object,
            _parsingSessionRepository,
            _albumParsedEventRepository,
            _imageUploadServiceMock.Object,
            _albumParsingJobLoggerMock.Object);
    }

    [Fact]
    public async Task RunParsingJob_WhenSessionIsNew_ShouldSaveParsedEventsToDatabase()
    {
        // Arrange
        var fakeAlbumParsedEvents = new List<AlbumParsedEvent>
        {
            new() { SKU = "SKU1", DistributorCode = DistributorCode.OsmoseProductions },
            new() { SKU = "SKU2", DistributorCode = DistributorCode.OsmoseProductions }
        };
        _parserMock = MocksFactory.CreateParserMock(fakeAlbumParsedEvents);

        // Act
        await _albumParsingJob.RunParserJob(_parserDataSource, CancellationToken.None);

        // Assert
        var parsingSessions = await _parsingSessionRepository.GetParsedAsync(CancellationToken.None);
        Assert.Single(parsingSessions);
        Assert.Equal(parsingSessions[0].ParsingStatus, AlbumParsingStatus.Parsed);

        var albumParsedEventList = await _albumParsedEventRepository.GeAsync(parsingSessions[0].Id, CancellationToken.None);
        Assert.Equal(albumParsedEventList.Count, 2);

        var albumParsedEvent1 = JsonSerializer.Deserialize<AlbumParsedEvent>(albumParsedEventList[0].EventPayload);
        Assert.Equal("SKU1", albumParsedEvent1.SKU);

        var albumParsedEvent2 = JsonSerializer.Deserialize<AlbumParsedEvent>(albumParsedEventList[1].EventPayload);
        Assert.Equal("SKU2", albumParsedEvent2!.SKU);
    }

    [Fact]
    public async Task RunParsingJob_WhenSessionExists_ShouldReuseSession()
    {
        // Arrange
        var fakeAlbumParsedEvents = new List<AlbumParsedEvent>
        {
            new() { SKU = "SKU3", DistributorCode = DistributorCode.OsmoseProductions }
        };
        _parserMock = MocksFactory.CreateParserMock(fakeAlbumParsedEvents);

        await _parsingSessionRepository.AddAsync(DistributorCode.OsmoseProductions, "https://www.test.com", CancellationToken.None);

        // Act
        await _albumParsingJob.RunParserJob(_parserDataSource, CancellationToken.None);

        // Assert
        var parsingSessions = await _parsingSessionRepository.GetParsedAsync(CancellationToken.None);
        Assert.Single(parsingSessions);
        Assert.Equal(parsingSessions[0].ParsingStatus, AlbumParsingStatus.Parsed);

        var albumParsedEventList = await _albumParsedEventRepository.GeAsync(parsingSessions[0].Id, CancellationToken.None);
        Assert.Single(albumParsedEventList);

        var albumParsedEvent1 = JsonSerializer.Deserialize<AlbumParsedEvent>(albumParsedEventList[0].EventPayload);
        Assert.Equal("SKU3", albumParsedEvent1.SKU);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _fixture.DbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"AlbumParsedEvents\", \"ParsingSessions\" RESTART IDENTITY CASCADE;");
    }
}