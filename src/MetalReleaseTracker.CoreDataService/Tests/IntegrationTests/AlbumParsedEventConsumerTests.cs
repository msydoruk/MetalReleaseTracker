using AutoMapper;
using MassTransit;
using MetalReleaseTracker.CoreDataService.Configuration;
using MetalReleaseTracker.CoreDataService.Consumers;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Events;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;
using MetalReleaseTracker.CoreDataService.Extensions;
using MetalReleaseTracker.CoreDataService.Tests.Factories;
using MetalReleaseTracker.CoreDataService.Tests.Fixtures;
using Moq;
using Xunit;

namespace MetalReleaseTracker.CoreDataService.Tests.IntegrationTests;

public class AlbumParsedEventConsumerTests : IClassFixture<TestPostgresDatabaseFixture>
{
    private readonly AlbumRepository _albumRepository;
    private readonly BandRepository _bandRepository;
    private readonly DistributorRepository _distributorRepository;
    private readonly Mock<ILogger<AlbumProcessedEventConsumer>> _albumProcessedEventConsumerLoggerMock;
    private readonly IMapper _mapper;
    private readonly AlbumProcessedEventConsumer _albumProcessedEventConsumer;

    public AlbumParsedEventConsumerTests(TestPostgresDatabaseFixture fixture)
    {
        _albumRepository = new AlbumRepository(fixture.DbContext);
        _bandRepository = new BandRepository(fixture.DbContext);
        _distributorRepository = new DistributorRepository(fixture.DbContext);
        _albumProcessedEventConsumerLoggerMock = new();
        _mapper = new MapperConfiguration(cfg => cfg.CreateMap<AlbumProcessedPublicationEvent, AlbumEntity>()).CreateMapper();

        _albumProcessedEventConsumer = new AlbumProcessedEventConsumer(
            _albumRepository,
            _bandRepository,
            _distributorRepository,
            _albumProcessedEventConsumerLoggerMock.Object,
            _mapper);
    }

    [Fact]
    public async Task Consume_WhenConsumingNewAlbum_ShouldAddAlbumToDatabase()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var albumProcessedPublicationEvent = AlbumFactory.CreateAlbumProcessedPublicationEvent(
            albumId,
            albumId.ToString(),
            DistributorCode.OsmoseProductions,
            "Test Band",
            10,
            AlbumProcessedStatus.New);

        var consumerContextMock =  new Mock<ConsumeContext<AlbumProcessedPublicationEvent>>();
        consumerContextMock.Setup(x => x.Message).Returns(albumProcessedPublicationEvent);

        // Act
        await _albumProcessedEventConsumer.Consume(consumerContextMock.Object);

        // Assert
        var newAlbum = await _albumRepository.GetAsync(albumProcessedPublicationEvent.Id);
        Assert.NotNull(newAlbum);
        Assert.Equal(newAlbum.SKU, albumProcessedPublicationEvent.SKU);

        var newBandId = await _bandRepository.GetOrAddAsync("Test Band");
        Assert.NotEqual(newBandId, Guid.Empty);

        var newDistributorId = await _distributorRepository.GetOrAddAsync(DistributorCode.OsmoseProductions.TryGetDisplayName());
        Assert.NotEqual(newDistributorId, Guid.Empty);
    }

    [Fact]
    public async Task Consume_WhenConsumingUpdatedAlbum_ShouldUpdateAlbumToDatabase()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        await AddAlbumToDatabase(albumId);

        var albumProcessedPublicationEvent = AlbumFactory.CreateAlbumProcessedPublicationEvent(
            albumId,
            albumId.ToString(),
            DistributorCode.OsmoseProductions,
            "Test Band",
            10,
            AlbumProcessedStatus.Updated);

        var consumerContextMock =  new Mock<ConsumeContext<AlbumProcessedPublicationEvent>>();
        consumerContextMock.Setup(x => x.Message).Returns(albumProcessedPublicationEvent);

        // Act
        await _albumProcessedEventConsumer.Consume(consumerContextMock.Object);

        // Assert
        var existingAlbum = await _albumRepository.GetAsync(albumProcessedPublicationEvent.Id);
        Assert.NotNull(existingAlbum);
        Assert.Equal(existingAlbum.Price, albumProcessedPublicationEvent.Price);
    }

    [Fact]
    public async Task Consume_WhenConsumingDeletedAlbum_ShouldDeleteAlbumFromDatabase()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        await AddAlbumToDatabase(albumId);

        var albumProcessedPublicationEvent = AlbumFactory.CreateAlbumProcessedPublicationEvent(
            albumId,
            albumId.ToString(),
            DistributorCode.OsmoseProductions,
            "Test Band",
            10,
            AlbumProcessedStatus.Deleted);

        var consumerContextMock =  new Mock<ConsumeContext<AlbumProcessedPublicationEvent>>();
        consumerContextMock.Setup(x => x.Message).Returns(albumProcessedPublicationEvent);

        // Act
        await _albumProcessedEventConsumer.Consume(consumerContextMock.Object);

        // Assert
        var deletedAlbum = await _albumRepository.GetAsync(albumProcessedPublicationEvent.Id);
        Assert.Null(deletedAlbum);
    }

    private async Task AddAlbumToDatabase(Guid albumId)
    {
        var distributorId = await _distributorRepository.GetOrAddAsync("Osmose Productions");
        var bandId = await _bandRepository.GetOrAddAsync("Test Band");

        var albumEntity = AlbumFactory.CreateAlbumEntity(albumId, albumId.ToString(), distributorId, bandId);
        await _albumRepository.AddAsync(albumEntity);
    }
}
