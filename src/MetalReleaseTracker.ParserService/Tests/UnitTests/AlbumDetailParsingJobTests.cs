using MetalReleaseTracker.ParserService.Domain.Interfaces;
using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Models;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.UnitTests;

public class AlbumDetailParsingJobTests
{
    private readonly Mock<IAlbumDetailParser> _parserMock = new();
    private readonly Mock<ICatalogueIndexRepository> _catalogueIndexRepoMock = new();
    private readonly Mock<ICatalogueIndexDetailRepository> _catalogueIndexDetailRepoMock = new();
    private readonly Mock<IImageUploadService> _imageUploadServiceMock = new();
    private readonly Mock<ISettingsService> _settingsServiceMock = new();
    private readonly Mock<IParsingProgressTracker> _progressTrackerMock = new();

    [Fact]
    public async Task ParseRelevantEntries_WithBandDiscography_SetsCanonicalFields()
    {
        var distributorCode = DistributorCode.OsmoseProductions;
        var discographyId = Guid.NewGuid();

        var catalogueEntry = new CatalogueIndexEntity
        {
            Id = Guid.NewGuid(),
            DistributorCode = distributorCode,
            BandName = "1914",
            AlbumTitle = "Where Fear and Weapons Meet",
            DetailUrl = "https://example.com/detail",
            Status = CatalogueIndexStatus.AiVerified,
            BandDiscographyId = discographyId,
            BandDiscography = new BandDiscographyEntity
            {
                Id = discographyId,
                BandReferenceId = Guid.NewGuid(),
                AlbumTitle = "Where Fear and Weapons Meet",
                NormalizedAlbumTitle = "where fear and weapons meet",
                AlbumType = "Full-length",
                Year = 2021,
            },
        };

        var parsedEvent = new AlbumParsedEvent
        {
            DistributorCode = distributorCode,
            BandName = "1914",
            SKU = "SKU-001",
            Name = "Where Fear and Weapons Meet (Digipak)",
            Price = 15.0f,
            PurchaseUrl = "https://example.com/buy",
            PhotoUrl = "https://example.com/photo.jpg",
            Label = "Napalm Records",
            Press = "EU",
        };

        CatalogueIndexDetailEntity? capturedDetail = null;

        SetupMocks(distributorCode, catalogueEntry, parsedEvent, detail => capturedDetail = detail);

        var job = CreateJob();
        var dataSource = new ParserDataSource
        {
            DistributorCode = distributorCode,
            Name = "Osmose",
            ParsingUrl = "https://example.com",
        };

        await job.RunDetailParsingJob(dataSource, CancellationToken.None);

        Assert.NotNull(capturedDetail);
        Assert.Equal("Where Fear and Weapons Meet", capturedDetail!.CanonicalTitle);
        Assert.Equal(2021, capturedDetail.OriginalYear);
        Assert.Equal(ChangeType.New, capturedDetail.ChangeType);
        Assert.Equal(PublicationStatus.Unpublished, capturedDetail.PublicationStatus);
    }

    [Fact]
    public async Task ParseRelevantEntries_WithoutBandDiscography_LeavesCanonicalFieldsNull()
    {
        var distributorCode = DistributorCode.OsmoseProductions;

        var catalogueEntry = new CatalogueIndexEntity
        {
            Id = Guid.NewGuid(),
            DistributorCode = distributorCode,
            BandName = "Some Band",
            AlbumTitle = "Some Album (Digipak CD)",
            DetailUrl = "https://example.com/detail",
            Status = CatalogueIndexStatus.AiVerified,
            BandDiscographyId = null,
            BandDiscography = null,
        };

        var parsedEvent = new AlbumParsedEvent
        {
            DistributorCode = distributorCode,
            BandName = "Some Band",
            SKU = "SKU-002",
            Name = "Some Album (Digipak CD)",
            Price = 12.0f,
            PurchaseUrl = "https://example.com/buy2",
            PhotoUrl = "https://example.com/photo2.jpg",
            Label = "Some Label",
            Press = "EU",
        };

        CatalogueIndexDetailEntity? capturedDetail = null;

        SetupMocks(distributorCode, catalogueEntry, parsedEvent, detail => capturedDetail = detail);

        var job = CreateJob();
        var dataSource = new ParserDataSource
        {
            DistributorCode = distributorCode,
            Name = "Osmose",
            ParsingUrl = "https://example.com",
        };

        await job.RunDetailParsingJob(dataSource, CancellationToken.None);

        Assert.NotNull(capturedDetail);
        Assert.Null(capturedDetail!.CanonicalTitle);
        Assert.Null(capturedDetail.OriginalYear);
    }

    [Fact]
    public async Task ParseRelevantEntries_WithBandDiscography_NullYear_SetsCanonicalTitleOnly()
    {
        var distributorCode = DistributorCode.Drakkar;
        var discographyId = Guid.NewGuid();

        var catalogueEntry = new CatalogueIndexEntity
        {
            Id = Guid.NewGuid(),
            DistributorCode = distributorCode,
            BandName = "TestBand",
            AlbumTitle = "TestAlbum",
            DetailUrl = "https://example.com/detail",
            Status = CatalogueIndexStatus.AiVerified,
            BandDiscographyId = discographyId,
            BandDiscography = new BandDiscographyEntity
            {
                Id = discographyId,
                BandReferenceId = Guid.NewGuid(),
                AlbumTitle = "Canonical Album Title",
                NormalizedAlbumTitle = "canonical album title",
                AlbumType = "Demo",
                Year = null,
            },
        };

        var parsedEvent = new AlbumParsedEvent
        {
            DistributorCode = distributorCode,
            BandName = "TestBand",
            SKU = "SKU-003",
            Name = "TestAlbum",
            Price = 10.0f,
            PurchaseUrl = "https://example.com/buy3",
            PhotoUrl = "https://example.com/photo3.jpg",
            Label = "Test Label",
            Press = "US",
        };

        CatalogueIndexDetailEntity? capturedDetail = null;

        SetupMocks(distributorCode, catalogueEntry, parsedEvent, detail => capturedDetail = detail);

        var job = CreateJob();
        var dataSource = new ParserDataSource
        {
            DistributorCode = distributorCode,
            Name = "Drakkar",
            ParsingUrl = "https://example.com",
        };

        await job.RunDetailParsingJob(dataSource, CancellationToken.None);

        Assert.NotNull(capturedDetail);
        Assert.Equal("Canonical Album Title", capturedDetail!.CanonicalTitle);
        Assert.Null(capturedDetail.OriginalYear);
    }

    private AlbumDetailParsingJob CreateJob()
    {
        return new AlbumDetailParsingJob(
            _ => _parserMock.Object,
            _catalogueIndexRepoMock.Object,
            _catalogueIndexDetailRepoMock.Object,
            _imageUploadServiceMock.Object,
            _settingsServiceMock.Object,
            _progressTrackerMock.Object,
            NullLogger<AlbumDetailParsingJob>.Instance);
    }

    private void SetupMocks(
        DistributorCode distributorCode,
        CatalogueIndexEntity catalogueEntry,
        AlbumParsedEvent parsedEvent,
        Action<CatalogueIndexDetailEntity> captureDetail)
    {
        _settingsServiceMock
            .Setup(settingsService => settingsService.GetParsingSourceByCodeAsync(distributorCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ParsingSourceEntity
            {
                Id = Guid.NewGuid(),
                DistributorCode = distributorCode,
                Name = "Test Source",
                ParsingUrl = "https://example.com",
                IsEnabled = true,
            });

        _settingsServiceMock
            .Setup(settingsService => settingsService.GetGeneralParserSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeneralParserSettings
            {
                MinDelayBetweenRequestsSeconds = 0,
                MaxDelayBetweenRequestsSeconds = 0,
            });

        _catalogueIndexRepoMock
            .Setup(repository => repository.GetByStatusesWithDiscographyAsync(
                distributorCode,
                It.IsAny<IEnumerable<CatalogueIndexStatus>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CatalogueIndexEntity> { catalogueEntry });

        _catalogueIndexDetailRepoMock
            .Setup(repository => repository.GetByCatalogueIndexIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CatalogueIndexDetailEntity?)null);

        _catalogueIndexDetailRepoMock
            .Setup(repository => repository.AddAsync(It.IsAny<CatalogueIndexDetailEntity>(), It.IsAny<CancellationToken>()))
            .Callback<CatalogueIndexDetailEntity, CancellationToken>((detail, _) => captureDetail(detail))
            .Returns(Task.CompletedTask);

        _parserMock
            .Setup(parser => parser.ParseAlbumDetailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsedEvent);

        _imageUploadServiceMock
            .Setup(service => service.UploadAlbumImageAsync(It.IsAny<ImageUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImageUploadResult.Success("blob/path.jpg", "https://example.com/photo.jpg"));
    }
}
