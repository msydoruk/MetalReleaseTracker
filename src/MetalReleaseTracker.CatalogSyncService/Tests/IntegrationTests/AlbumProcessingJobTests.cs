using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Implementation;
using MetalReleaseTracker.CatalogSyncService.Services.Jobs;
using MetalReleaseTracker.CatalogSyncService.Tests.Fixtures;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MetalReleaseTracker.CatalogSyncService.Tests.IntegrationTests;

 public class AlbumProcessingJobIntegrationTests : IClassFixture<TestMongoDatabaseFixture>
    {
        private readonly AlbumProcessedRepository _albumProcessedRepository;
        private readonly ParsingSessionWithRawAlbumsRepository _parsingSessionRepository;
        private readonly Mock<IValidator<RawAlbumEntity>> _rawAlbumValidatorMock;
        private readonly IMapper _mapper;
        private readonly Mock<ILogger<AlbumProcessingJob>> _albumProcessingJobLoggerMock;
        private readonly AlbumProcessingJob _albumProcessingJob;

        public AlbumProcessingJobIntegrationTests(TestMongoDatabaseFixture fixture)
        {
            _albumProcessedRepository = new AlbumProcessedRepository(fixture.MongoDatabase, fixture.MongoDbConfig);
            _parsingSessionRepository = new ParsingSessionWithRawAlbumsRepository(fixture.MongoDatabase, fixture.MongoDbConfig);
            _rawAlbumValidatorMock = new Mock<IValidator<RawAlbumEntity>>();
            _mapper = new MapperConfiguration(cfg => cfg.CreateMap<RawAlbumEntity, AlbumProcessedEntity>()).CreateMapper();
            _albumProcessingJobLoggerMock = new();

            _albumProcessingJob = new AlbumProcessingJob(
                _parsingSessionRepository,
                _rawAlbumValidatorMock.Object,
                _albumProcessedRepository,
                _mapper,
                _albumProcessingJobLoggerMock.Object);
        }

        [Fact]
        public async Task RunProcessingJob_WhenProcessingNewAlbums_ShouldProcessNewAlbums()
        {
            // Arrange
            var parsingSessionId = Guid.NewGuid();

            var rawAlbums = new List<RawAlbumEntity>
            {
                new RawAlbumEntity { SKU = "SKU_NEW1", DistributorCode = DistributorCode.OsmoseProductions, Price = 10 },
                new RawAlbumEntity { SKU = "SKU_NEW2", DistributorCode = DistributorCode.OsmoseProductions, Price = 11 }
            };

            var parsingSession = new ParsingSessionWithRawAlbumsEntity
            {
                Id = parsingSessionId,
                DistributorCode = DistributorCode.OsmoseProductions,
                RawAlbums = rawAlbums
            };

            await _parsingSessionRepository.AddAsync(parsingSession);

            _rawAlbumValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<RawAlbumEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            await _albumProcessingJob.RunProcessingJob(CancellationToken.None);

            // Assert
            var newAlbum1 = await _albumProcessedRepository.GetBySkuAsync("SKU_NEW1", CancellationToken.None);
            Assert.NotNull(newAlbum1);
            Assert.Equal(10, newAlbum1.Price);
            Assert.Equal(AlbumProcessedStatus.New, newAlbum1.ProcessedStatus);

            var newAlbum2 = await _albumProcessedRepository.GetBySkuAsync("SKU_NEW2", CancellationToken.None);
            Assert.NotNull(newAlbum2);
            Assert.Equal(11, newAlbum2.Price);
            Assert.Equal(AlbumProcessedStatus.New, newAlbum2.ProcessedStatus);
        }

        [Fact]
        public async Task RunProcessingJob_WhenProcessingUpdatedAlbums_ShouldProcessUpdatedAlbums()
        {
            // Arrange
            var parsingSessionId = Guid.NewGuid();

            var rawAlbums = new List<RawAlbumEntity>
            {
                new RawAlbumEntity { SKU = "SKU_EXIST1", DistributorCode = DistributorCode.OsmoseProductions, Price = 10 },
                new RawAlbumEntity { SKU = "SKU_EXIST2", DistributorCode = DistributorCode.OsmoseProductions, Price = 20 }
            };

            var existingAlbum1 = new AlbumProcessedEntity
            {
                Id = Guid.NewGuid(),
                SKU = "SKU_EXIST1",
                DistributorCode = DistributorCode.OsmoseProductions,
                Price = 15,
                ProcessedStatus = AlbumProcessedStatus.Published
            };

            var existingAlbum2 = new AlbumProcessedEntity
            {
                Id = Guid.NewGuid(),
                SKU = "SKU_EXIST2",
                DistributorCode = DistributorCode.OsmoseProductions,
                Price = 25,
                ProcessedStatus = AlbumProcessedStatus.Published
            };

            await _albumProcessedRepository.AddAsync(existingAlbum1, CancellationToken.None);
            await _albumProcessedRepository.AddAsync(existingAlbum2, CancellationToken.None);

            var parsingSession = new ParsingSessionWithRawAlbumsEntity
            {
                Id = parsingSessionId,
                DistributorCode = DistributorCode.OsmoseProductions,
                RawAlbums = rawAlbums
            };

            await _parsingSessionRepository.AddAsync(parsingSession);

            _rawAlbumValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<RawAlbumEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            await _albumProcessingJob.RunProcessingJob(CancellationToken.None);

            // Assert
            var updatedAlbum1 = await _albumProcessedRepository.GetBySkuAsync("SKU_EXIST1", CancellationToken.None);
            Assert.NotNull(updatedAlbum1);
            Assert.Equal(10, updatedAlbum1.Price);
            Assert.Equal(AlbumProcessedStatus.Updated, updatedAlbum1.ProcessedStatus);

            var updatedAlbum2 = await _albumProcessedRepository.GetBySkuAsync("SKU_EXIST2", CancellationToken.None);
            Assert.NotNull(updatedAlbum2);
            Assert.Equal(20, updatedAlbum2.Price);
            Assert.Equal(AlbumProcessedStatus.Updated, updatedAlbum2.ProcessedStatus);
        }

        [Fact]
        public async Task RunProcessingJob_WhenProcessingDeletedAlbums_ShouldProcessDeletedAlubms()
        {
            // Arrange
            var parsingSessionId = Guid.NewGuid();

            var rawAlbums = new List<RawAlbumEntity>
            {
                new RawAlbumEntity { SKU = "SKU_NEW", DistributorCode = DistributorCode.OsmoseProductions, Price = 10 }
            };

            var existingAlbum1 = new AlbumProcessedEntity
            {
                Id = Guid.NewGuid(),
                SKU = "SKU_DELETE1",
                DistributorCode = DistributorCode.OsmoseProductions,
                Price = 15,
                ProcessedStatus = AlbumProcessedStatus.Published
            };

            var existingAlbum2 = new AlbumProcessedEntity
            {
                Id = Guid.NewGuid(),
                SKU = "SKU_DELETE2",
                DistributorCode = DistributorCode.OsmoseProductions,
                ProcessedStatus = AlbumProcessedStatus.Published
            };

            await _albumProcessedRepository.AddAsync(existingAlbum1, CancellationToken.None);
            await _albumProcessedRepository.AddAsync(existingAlbum2, CancellationToken.None);

            var parsingSession = new ParsingSessionWithRawAlbumsEntity
            {
                Id = parsingSessionId,
                DistributorCode = DistributorCode.OsmoseProductions,
                RawAlbums = rawAlbums
            };

            await _parsingSessionRepository.AddAsync(parsingSession);

            _rawAlbumValidatorMock.Setup(x => x.ValidateAsync(It.IsAny<RawAlbumEntity>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            // Act
            await _albumProcessingJob.RunProcessingJob(CancellationToken.None);

            // Assert
            var deletedAlbum1 = await _albumProcessedRepository.GetBySkuAsync("SKU_DELETE1", CancellationToken.None);
            Assert.NotNull(deletedAlbum1);
            Assert.Equal(AlbumProcessedStatus.Deleted, deletedAlbum1.ProcessedStatus);

            var deletedAlbum2 = await _albumProcessedRepository.GetBySkuAsync("SKU_DELETE1", CancellationToken.None);
            Assert.NotNull(deletedAlbum2);
            Assert.Equal(AlbumProcessedStatus.Deleted, deletedAlbum2.ProcessedStatus);
        }
    }