using AutoMapper;
using BenchmarkDotNet.Attributes;
using FluentValidation;
using FluentValidation.Results;
using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Implementation;
using MetalReleaseTracker.CatalogSyncService.Services.Jobs;
using MetalReleaseTracker.CatalogSyncService.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;

namespace MetalReleaseTracker.Benchmarks.CatalogSyncService
{
    [MemoryDiagnoser]
    public class AlbumProcessingJobBenchmarks
    {
        private TestMongoDatabaseFixture _fixture;
        private AlbumProcessedRepository _albumProcessedRepository;
        private ParsingSessionWithRawAlbumsRepository _parsingSessionRepository;
        private Mock<IValidator<RawAlbumEntity>> _validator;
        private IMapper _mapper;
        private Mock<ILogger<AlbumProcessingJob>> _logger;
        private AlbumProcessingJob _job;

        [ParamsAllValues]
        public AlbumProcessedStatus Scenario { get; set; }

        [Params(10, 100, 1000)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fixture = new TestMongoDatabaseFixture();
            _fixture.InitializeAsync().GetAwaiter().GetResult();

            _albumProcessedRepository = new AlbumProcessedRepository(_fixture.MongoDatabase, _fixture.MongoDbConfig);
            _parsingSessionRepository = new ParsingSessionWithRawAlbumsRepository(_fixture.MongoDatabase, _fixture.MongoDbConfig);

            _validator = new Mock<IValidator<RawAlbumEntity>>();
            _validator
                .Setup(x => x.ValidateAsync(
                    It.IsAny<RawAlbumEntity>(), 
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<RawAlbumEntity, AlbumProcessedEntity>();
            }).CreateMapper();

            _logger = new Mock<ILogger<AlbumProcessingJob>>();

            _job = new AlbumProcessingJob(
                _parsingSessionRepository,
                _validator.Object,
                _albumProcessedRepository,
                _mapper,
                _logger.Object);

            ClearCollections().GetAwaiter().GetResult();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            ClearCollections().GetAwaiter().GetResult();

            switch (Scenario)
            {
                case AlbumProcessedStatus.New:
                    SeedNew(DataSize).GetAwaiter().GetResult();
                    break;
                case AlbumProcessedStatus.Updated:
                    SeedUpdated(DataSize).GetAwaiter().GetResult();
                    break;
                case AlbumProcessedStatus.Deleted:
                    SeedDeleted(DataSize).GetAwaiter().GetResult();
                    break;
            }
        }

        [Benchmark]
        public void ProcessAlbums()
        {
            _job.RunProcessingJob(CancellationToken.None).GetAwaiter().GetResult();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            ClearCollections().GetAwaiter().GetResult();
            _fixture?.DisposeAsync().GetAwaiter().GetResult();
        }

        private async Task ClearCollections()
        {
            await _fixture.MongoDatabase.DropCollectionAsync(
                _fixture.MongoDbConfig.Value.ParsingSessionWithRawAlbumsCollectionName);
            await _fixture.MongoDatabase.DropCollectionAsync(
                _fixture.MongoDbConfig.Value.ProcessedAlbumsCollectionName);
        }

        private async Task SeedNew(int size)
        {
            var raw = new List<RawAlbumEntity>(size);
            for (int i = 0; i < size; i++)
            {
                raw.Add(new RawAlbumEntity
                {
                    SKU = $"NEW_{i}",
                    DistributorCode = DistributorCode.OsmoseProductions,
                    Price = i
                });
            }

            await _parsingSessionRepository.AddAsync(
                new ParsingSessionWithRawAlbumsEntity
                {
                    Id = Guid.NewGuid(),
                    DistributorCode = DistributorCode.OsmoseProductions,
                    RawAlbums = raw
                });
        }

        private async Task SeedUpdated(int size)
        {
            for (int i = 0; i < size; i++)
            {
                await _albumProcessedRepository.AddAsync(
                    new AlbumProcessedEntity
                    {
                        Id = Guid.NewGuid(),
                        SKU = $"UPD_{i}",
                        DistributorCode = DistributorCode.OsmoseProductions,
                        Price = i + 100,
                        ProcessedStatus = AlbumProcessedStatus.Published
                    },
                    CancellationToken.None);
            }

            var raw = new List<RawAlbumEntity>(size);
            for (int i = 0; i < size; i++)
            {
                raw.Add(new RawAlbumEntity
                {
                    SKU = $"UPD_{i}",
                    DistributorCode = DistributorCode.OsmoseProductions,
                    Price = i
                });
            }

            await _parsingSessionRepository.AddAsync(
                new ParsingSessionWithRawAlbumsEntity
                {
                    Id = Guid.NewGuid(),
                    DistributorCode = DistributorCode.OsmoseProductions,
                    RawAlbums = raw
                });
        }

        private async Task SeedDeleted(int size)
        {
            for (int i = 0; i < size; i++)
            {
                await _albumProcessedRepository.AddAsync(
                    new AlbumProcessedEntity
                    {
                        Id = Guid.NewGuid(),
                        SKU = $"DEL_{i}",
                        DistributorCode = DistributorCode.OsmoseProductions,
                        Price = i,
                        ProcessedStatus = AlbumProcessedStatus.Published
                    },
                    CancellationToken.None);
            }

            await _parsingSessionRepository.AddAsync(
                new ParsingSessionWithRawAlbumsEntity
                {
                    Id = Guid.NewGuid(),
                    DistributorCode = DistributorCode.OsmoseProductions,
                    RawAlbums = new List<RawAlbumEntity>
                    {
                        new RawAlbumEntity
                        {
                            SKU = "DEL_NOT_PRESENT",
                            DistributorCode = DistributorCode.OsmoseProductions,
                            Price = 999
                        }
                    }
                });
        }
    }
}