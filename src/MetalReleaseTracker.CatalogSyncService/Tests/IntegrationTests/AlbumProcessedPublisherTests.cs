using AutoMapper;
using MassTransit;
using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MetalReleaseTracker.CatalogSyncService.Data.Events;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Implementation;
using MetalReleaseTracker.CatalogSyncService.Services.Jobs;
using MetalReleaseTracker.CatalogSyncService.Tests.Fixtures;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MetalReleaseTracker.CatalogSyncService.Tests.IntegrationTests;

public class AlbumProcessedPublisherTests : IClassFixture<TestMongoDatabaseFixture>
{
    private readonly Mock<ITopicProducer<AlbumProcessedPublicationEvent>> _topicProducerMock = new();
    private readonly AlbumProcessedRepository _albumProcessedRepository;
    private readonly Mock<ILogger<AlbumProcessedPublisherJob>> _albumProcessedPublisherJobLoggerMock;
    private readonly IMapper _mapper;
    private readonly AlbumProcessedPublisherJob _albumProcessedPublisherJob;

    public AlbumProcessedPublisherTests(TestMongoDatabaseFixture fixture)
    {
        _topicProducerMock.Setup(x => x.Produce(It.IsAny<AlbumProcessedPublicationEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _albumProcessedRepository = new AlbumProcessedRepository(fixture.MongoDatabase, fixture.MongoDbConfig);
        _mapper  = new MapperConfiguration(cfg => cfg.CreateMap<AlbumProcessedEntity, AlbumProcessedPublicationEvent>()).CreateMapper();
        _albumProcessedPublisherJobLoggerMock = new();
        var options = Options.Create(new AlbumProcessedPublisherJobSettings { BatchSize = 10 });

        _albumProcessedPublisherJob = new AlbumProcessedPublisherJob(
            _topicProducerMock.Object,
            _albumProcessedRepository,
            _albumProcessedPublisherJobLoggerMock.Object,
            options,
            _mapper);
    }

    [Fact]
    public async Task RunPublisherJob_WhenAlbumsAreProcessed_ShouldPublishAlbums()
    {
        // Arrange
        var albumProcessedEntities = new List<AlbumProcessedEntity>
        {
            new AlbumProcessedEntity { SKU = "SKU1", DistributorCode = DistributorCode.OsmoseProductions, ProcessedStatus = AlbumProcessedStatus.New },
            new AlbumProcessedEntity { SKU = "SKU2", DistributorCode = DistributorCode.OsmoseProductions, ProcessedStatus = AlbumProcessedStatus.Updated },
            new AlbumProcessedEntity { SKU = "SKU3", DistributorCode = DistributorCode.OsmoseProductions, ProcessedStatus = AlbumProcessedStatus.Deleted },
            new AlbumProcessedEntity { SKU = "SKU4", DistributorCode = DistributorCode.OsmoseProductions, ProcessedStatus = AlbumProcessedStatus.Published },
        };

        await _albumProcessedRepository.AddAsync(albumProcessedEntities[0], CancellationToken.None);
        await _albumProcessedRepository.AddAsync(albumProcessedEntities[1], CancellationToken.None);
        await _albumProcessedRepository.AddAsync(albumProcessedEntities[2], CancellationToken.None);
        await _albumProcessedRepository.AddAsync(albumProcessedEntities[3], CancellationToken.None);

        // Act
        await _albumProcessedPublisherJob.RunPublisherJob(CancellationToken.None);

        // Assert
        _topicProducerMock.Verify(x => x.Produce(It.IsAny<AlbumProcessedPublicationEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(3));

        var unPublishedAlbums = await _albumProcessedRepository.GetUnPublishedBatchAsync(10, CancellationToken.None);
        Assert.Empty(unPublishedAlbums);
    }
}