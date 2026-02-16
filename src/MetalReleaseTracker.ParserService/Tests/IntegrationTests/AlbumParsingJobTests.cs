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
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.IntegrationTests;

public class AlbumDetailParsingJobTests : IClassFixture<TestPostgresDatabaseFixture>, IAsyncLifetime
{
    private readonly TestPostgresDatabaseFixture _fixture;
    private readonly ParsingSessionRepository _parsingSessionRepository;
    private readonly AlbumParsedEventRepository _albumParsedEventRepository;
    private readonly CatalogueIndexRepository _catalogueIndexRepository;
    private readonly Mock<ILogger<AlbumDetailParsingJob>> _loggerMock = new();
    private readonly Mock<ILogger<ParsingSessionRepository>> _parsingSessionRepositoryLoggerMock = new();
    private readonly Mock<IImageUploadService> _imageUploadServiceMock = new();
    private readonly Mock<IAlbumDetailParser> _detailParserMock = new();

    public AlbumDetailParsingJobTests(TestPostgresDatabaseFixture fixture)
    {
        _fixture = fixture;
        _parsingSessionRepository = new ParsingSessionRepository(_fixture.DbContext, _parsingSessionRepositoryLoggerMock.Object);
        _albumParsedEventRepository = new AlbumParsedEventRepository(_fixture.DbContext);
        _catalogueIndexRepository = new CatalogueIndexRepository(_fixture.DbContext);
    }

    [Fact]
    public async Task RunDetailParsingJob_WhenRelevantEntriesExist_ShouldParseAndMarkProcessed()
    {
        // Arrange
        var relevantEntry = new CatalogueIndexEntity
        {
            Id = Guid.NewGuid(),
            DistributorCode = DistributorCode.OsmoseProductions,
            BandName = "Drudkh",
            AlbumTitle = "Autumn Aurora",
            RawTitle = "Drudkh - Autumn Aurora",
            DetailUrl = "https://www.test.com/album1",
            Status = CatalogueIndexStatus.Relevant,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var notRelevantEntry = new CatalogueIndexEntity
        {
            Id = Guid.NewGuid(),
            DistributorCode = DistributorCode.OsmoseProductions,
            BandName = "Mayhem",
            AlbumTitle = "De Mysteriis",
            RawTitle = "Mayhem - De Mysteriis",
            DetailUrl = "https://www.test.com/album2",
            Status = CatalogueIndexStatus.NotRelevant,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _fixture.DbContext.CatalogueIndex.Add(relevantEntry);
        _fixture.DbContext.CatalogueIndex.Add(notRelevantEntry);
        await _fixture.DbContext.SaveChangesAsync();

        _detailParserMock
            .Setup(x => x.ParseAlbumDetailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AlbumParsedEvent
            {
                SKU = "SKU1",
                BandName = "Drudkh",
                Name = "Autumn Aurora",
                DistributorCode = DistributorCode.OsmoseProductions
            });

        var generalParserSettingsMock = new Mock<IOptions<GeneralParserSettings>>();
        generalParserSettingsMock.Setup(x => x.Value).Returns(new GeneralParserSettings
        {
            MinDelayBetweenRequestsSeconds = 0,
            MaxDelayBetweenRequestsSeconds = 1
        });

        var parserDataSource = new ParserDataSource
        {
            DistributorCode = DistributorCode.OsmoseProductions,
            Name = "test",
            ParsingUrl = "https://test.com"
        };

        var job = new AlbumDetailParsingJob(
            _ => _detailParserMock.Object,
            _catalogueIndexRepository,
            _parsingSessionRepository,
            _albumParsedEventRepository,
            _imageUploadServiceMock.Object,
            generalParserSettingsMock.Object,
            _loggerMock.Object);

        // Act
        await job.RunDetailParsingJob(parserDataSource, CancellationToken.None);

        // Assert
        var processedEntry = await _fixture.DbContext.CatalogueIndex
            .FirstOrDefaultAsync(e => e.Id == relevantEntry.Id);
        Assert.Equal(CatalogueIndexStatus.Processed, processedEntry!.Status);

        var skippedEntry = await _fixture.DbContext.CatalogueIndex
            .FirstOrDefaultAsync(e => e.Id == notRelevantEntry.Id);
        Assert.Equal(CatalogueIndexStatus.NotRelevant, skippedEntry!.Status);

        var parsingSessions = await _parsingSessionRepository.GetParsedAsync(CancellationToken.None);
        Assert.Single(parsingSessions);
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        await _fixture.DbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"AlbumParsedEvents\", \"ParsingSessions\", \"CatalogueIndex\", \"BandReferences\" RESTART IDENTITY CASCADE;");
    }
}
