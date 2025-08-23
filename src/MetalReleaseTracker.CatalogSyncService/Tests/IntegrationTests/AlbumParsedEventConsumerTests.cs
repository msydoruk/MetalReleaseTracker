using MassTransit;
using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Consumers;
using MetalReleaseTracker.CatalogSyncService.Data.Events;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Implementation;
using MetalReleaseTracker.CatalogSyncService.Tests.Fixtures;
using MetalReleaseTracker.SharedLibraries.Minio;
using Moq;
using Xunit;

namespace MetalReleaseTracker.CatalogSyncService.Tests.IntegrationTests;

public class AlbumParsedEventConsumerTests : IClassFixture<TestMongoDatabaseFixture>
{
    private readonly ParsingSessionWithRawAlbumsRepository _parsingSessionWithRawAlbumsRepository;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<ILogger<AlbumParsedEventConsumer>> _albumParsedEventConsumerLoggerMock;

    public AlbumParsedEventConsumerTests(TestMongoDatabaseFixture fixture)
    {
        _parsingSessionWithRawAlbumsRepository = new ParsingSessionWithRawAlbumsRepository(fixture.MongoDatabase, fixture.MongoDbConfig);
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _albumParsedEventConsumerLoggerMock = new Mock<ILogger<AlbumParsedEventConsumer>>();
    }

    [Fact]
    public async Task Consume_WhenAlbumsParsed_ShouldSaveDataToMongoWithPendingStatus()
    {
        // Arrange
        var parsingSessionId = Guid.NewGuid();
        var storageFilePaths = new List<string> { "file1.json", "file2.json" };
        var rawAlbumJsonPart1 =
            "[{\"Id\":\"11111111-1111-1111-1111-111111111111\",\"ParsingSessionId\":\"22222222-2222-2222-2222-222222222222\",\"SKU\":\"SKU1\"}";
        var rawAlbumJsonPart2 =
            ",{\"Id\":\"33333333-3333-3333-3333-333333333333\",\"ParsingSessionId\":\"22222222-2222-2222-2222-222222222222\",\"SKU\":\"SKU2\"}]";

        _fileStorageServiceMock.SetupSequence(x => x.DownloadFileAsStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(rawAlbumJsonPart1)
            .ReturnsAsync(rawAlbumJsonPart2);

        var consumer = new AlbumParsedEventConsumer(
            _parsingSessionWithRawAlbumsRepository,
            _fileStorageServiceMock.Object,
            _albumParsedEventConsumerLoggerMock.Object);

        var albumParsedPublicationEvent = new AlbumParsedPublicationEvent
        {
            ParsingSessionId = parsingSessionId,
            DistributorCode = DistributorCode.OsmoseProductions,
            CreatedDate = DateTime.UtcNow,
            StorageFilePaths = storageFilePaths
        };

        var consumerContextMock = new Mock<ConsumeContext<AlbumParsedPublicationEvent>>();
        consumerContextMock.Setup(x => x.Message).Returns(albumParsedPublicationEvent);

        // Act
        await consumer.Consume(consumerContextMock.Object);

        // Assert
        var result = await _parsingSessionWithRawAlbumsRepository.GetUnProcessedAsync();
        Assert.Single(result);
        Assert.Equal(parsingSessionId, result[0].Id);
        Assert.Equal(DistributorCode.OsmoseProductions, result[0].DistributorCode);
        Assert.Equal(2, result[0].RawAlbums.Count);
        Assert.Equal("SKU1", result[0].RawAlbums[0].SKU);
        Assert.Equal("SKU2", result[0].RawAlbums[1].SKU);

        _fileStorageServiceMock.Verify(x => x.DownloadFileAsStringAsync("file1.json", It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.DownloadFileAsStringAsync("file2.json", It.IsAny<CancellationToken>()), Times.Once);
    }
}
