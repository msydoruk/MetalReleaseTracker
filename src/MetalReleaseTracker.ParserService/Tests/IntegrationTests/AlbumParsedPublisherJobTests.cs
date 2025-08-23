using System.Text;
using MassTransit;
using MetalReleaseTracker.ParserService.Domain.Models.Events;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Repositories;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs;
using MetalReleaseTracker.ParserService.Tests.Factories;
using MetalReleaseTracker.ParserService.Tests.Fixtures;
using MetalReleaseTracker.SharedLibraries.Minio;
using Moq;
using Newtonsoft.Json;
using Xunit;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MetalReleaseTracker.ParserService.Tests.IntegrationTests;

public class AlbumParsedPublisherJobTests : IClassFixture<TestPostgresDatabaseFixture>
{
    private readonly ParsingSessionRepository _parsingSessionRepository;
    private readonly AlbumParsedEventRepository _albumParsedEventRepository;
    private readonly Mock<ILogger<AlbumParsedPublisherJob>> _albumParsedPublisherJobLoggerMock = new();
    private readonly Mock<ILogger<ParsingSessionRepository>> _parsingSessionRepositoryLoggerMock = new();
    private readonly Mock<ITopicProducer<AlbumParsedPublicationEvent>> _topicProducerMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;

    public AlbumParsedPublisherJobTests(TestPostgresDatabaseFixture fixture)
    {
        _parsingSessionRepository = new ParsingSessionRepository(fixture.DbContext, _parsingSessionRepositoryLoggerMock.Object);
        _albumParsedEventRepository = new AlbumParsedEventRepository(fixture.DbContext);
        _topicProducerMock = MocksFactory.CreateTopicProducerMock();
        _fileStorageServiceMock = MocksFactory.CreateFileStorageServiceMock();
    }

    [Fact]
    public async Task RunPublisherJob_WhenSessionIsParsed_ShouldPublishAndUpdateSession()
    {
        // Arrange
        var parsingSession = await _parsingSessionRepository.AddAsync(DistributorCode.OsmoseProductions, "https://www.test.com", CancellationToken.None);

        var eventPayload = JsonConvert.SerializeObject(new AlbumParsedEvent
        {
            SKU = "SKU1",
            DistributorCode = DistributorCode.OsmoseProductions,
        });
        await _albumParsedEventRepository.AddAsync(parsingSession.Id, eventPayload, CancellationToken.None);
        await _parsingSessionRepository.UpdateParsingStatus(parsingSession.Id, AlbumParsingStatus.Parsed, CancellationToken.None);

        var albumParsedEvents = await _albumParsedEventRepository.GeAsync(parsingSession.Id, CancellationToken.None);
        var serializedAlbumParsedEvents = JsonSerializer.Serialize(albumParsedEvents);
        var optionsMock = MocksFactory.CreateAlbumParsedPublisherJobSettingsMock(Encoding.UTF8.GetByteCount(serializedAlbumParsedEvents));

        var albumParsedPublisherJob = new AlbumParsedPublisherJob(
            _albumParsedEventRepository,
            _parsingSessionRepository,
            _fileStorageServiceMock.Object,
            _topicProducerMock.Object,
            _albumParsedPublisherJobLoggerMock.Object,
            optionsMock.Object);

        // Act
        await albumParsedPublisherJob.RunPublisherJob(CancellationToken.None);

        // Assert
        _topicProducerMock.Verify(x => x.Produce(It.IsAny<AlbumParsedPublicationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Once);

        var updatedParsingSession = await _parsingSessionRepository.GetById(parsingSession.Id, CancellationToken.None);
        Assert.Equal(AlbumParsingStatus.Published, updatedParsingSession.ParsingStatus);
    }

    [Fact]
    public async Task RunPublisherJob_WhenEventExceedsMaxChunkSize_ShouldUploadTwoFiles()
    {
        // Arrange
        var parsingSession = await _parsingSessionRepository.AddAsync(DistributorCode.OsmoseProductions, "https://www.test.com", CancellationToken.None);

        var eventPayload1 = JsonSerializer.Serialize(new AlbumParsedEvent
        {
            SKU = "SKU1",
            DistributorCode = DistributorCode.OsmoseProductions,
        });

        var eventPayload2 = JsonSerializer.Serialize(new AlbumParsedEvent
        {
            SKU = "SKU2",
            DistributorCode = DistributorCode.OsmoseProductions,
        });

        await _albumParsedEventRepository.AddAsync(parsingSession.Id, eventPayload1, CancellationToken.None);
        await _albumParsedEventRepository.AddAsync(parsingSession.Id, eventPayload2, CancellationToken.None);
        await _parsingSessionRepository.UpdateParsingStatus(parsingSession.Id, AlbumParsingStatus.Parsed, CancellationToken.None);

        var albumParsedEvents = await _albumParsedEventRepository.GeAsync(parsingSession.Id, CancellationToken.None);
        var serializedAlbumParsedEvents = JsonConvert.SerializeObject(albumParsedEvents);
        var optionsMock = MocksFactory.CreateAlbumParsedPublisherJobSettingsMock(Encoding.UTF8.GetByteCount(serializedAlbumParsedEvents) / 2);

        var albumParsedPublisherJob = new AlbumParsedPublisherJob(
            _albumParsedEventRepository,
            _parsingSessionRepository,
            _fileStorageServiceMock.Object,
            _topicProducerMock.Object,
            _albumParsedPublisherJobLoggerMock.Object,
            optionsMock.Object);

        // Act
        await albumParsedPublisherJob.RunPublisherJob(CancellationToken.None);

        // Assert
        _topicProducerMock.Verify(x => x.Produce(It.IsAny<AlbumParsedPublicationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _fileStorageServiceMock.Verify(x => x.UploadFileAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()), Times.Exactly(2));

        var updatedParsingSession = await _parsingSessionRepository.GetById(parsingSession.Id, CancellationToken.None);
        Assert.Equal(AlbumParsingStatus.Published, updatedParsingSession.ParsingStatus);
    }
}