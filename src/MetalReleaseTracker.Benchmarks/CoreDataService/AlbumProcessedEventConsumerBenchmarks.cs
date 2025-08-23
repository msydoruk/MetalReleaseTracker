using AutoMapper;
using BenchmarkDotNet.Attributes;
using MassTransit;
using MetalReleaseTracker.CoreDataService.Configuration;
using MetalReleaseTracker.CoreDataService.Consumers;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Events;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;
using MetalReleaseTracker.CoreDataService.Tests.Factories;
using MetalReleaseTracker.CoreDataService.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace MetalReleaseTracker.Benchmarks.CoreDataService
{
    [MemoryDiagnoser]
    public class AlbumProcessedEventConsumerBenchmarks
    {
        private TestPostgresDatabaseFixture _fixture;
        private AlbumRepository _albumRepository;
        private BandRepository _bandRepository;
        private DistributorRepository _distributorRepository;
        private Mock<ILogger<AlbumProcessedEventConsumer>> _loggerMock;
        private IMapper _mapper;
        private AlbumProcessedEventConsumer _consumer;
        
        private List<ConsumeContext<AlbumProcessedPublicationEvent>> _eventsToConsume;

        [ParamsAllValues]
        public AlbumProcessedStatus Scenario { get; set; }

        [Params(10, 100, 1000)]
        public int DataSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _fixture = new TestPostgresDatabaseFixture();
            _fixture.InitializeAsync().GetAwaiter().GetResult();
            
            _albumRepository = new AlbumRepository(_fixture.DbContext);
            _bandRepository = new BandRepository(_fixture.DbContext);
            _distributorRepository = new DistributorRepository(_fixture.DbContext);
            _loggerMock = new Mock<ILogger<AlbumProcessedEventConsumer>>();
            _mapper = new MapperConfiguration(cfg => cfg.CreateMap<AlbumProcessedPublicationEvent, AlbumEntity>()).CreateMapper();

            _consumer = new AlbumProcessedEventConsumer(
                _albumRepository,
                _bandRepository,
                _distributorRepository,
                _loggerMock.Object,
                _mapper);

            ClearDatabase().GetAwaiter().GetResult();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            ClearDatabase().GetAwaiter().GetResult();
            
            if (Scenario == AlbumProcessedStatus.Updated || Scenario == AlbumProcessedStatus.Deleted)
            {
                for (int i = 0; i < DataSize; i++)
                {
                    var existingAlbumId = Guid.NewGuid();
                    AddAlbumToDatabase(existingAlbumId).GetAwaiter().GetResult();
                }
            }
            
            _eventsToConsume = new List<ConsumeContext<AlbumProcessedPublicationEvent>>(DataSize);

            for (int i = 0; i < DataSize; i++)
            {
                var albumId = Guid.NewGuid();
                var evt = AlbumFactory.CreateAlbumProcessedPublicationEvent(
                    albumId,
                    albumId.ToString(),
                    DistributorCode.OsmoseProductions,
                    "Test Band",
                    10 + i,
                    Scenario);

                var contextMock = new Mock<ConsumeContext<AlbumProcessedPublicationEvent>>();
                contextMock.Setup(x => x.Message).Returns(evt);
                _eventsToConsume.Add(contextMock.Object);
            }
        }

        [Benchmark]
        public void ConsumeAlbumEvents()
        {
            foreach (var ctx in _eventsToConsume)
            {
                _consumer.Consume(ctx).GetAwaiter().GetResult();
            }
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
                "TRUNCATE TABLE \"Albums\", \"Bands\", \"Distributors\" RESTART IDENTITY CASCADE;"
            );
        }

        private async Task AddAlbumToDatabase(Guid albumId)
        {
            var distributorId = await _distributorRepository.GetOrAddAsync("Osmose Productions");
            var bandId = await _bandRepository.GetOrAddAsync("Test Band");

            var albumEntity = AlbumFactory.CreateAlbumEntity(
                albumId,
                albumId.ToString(),
                distributorId,
                bandId);

            await _albumRepository.AddAsync(albumEntity);
        }
    }
}