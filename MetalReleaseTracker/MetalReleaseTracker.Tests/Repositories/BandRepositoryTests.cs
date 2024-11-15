using AutoMapper;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;
using MetalReleaseTracker.Infrastructure.Repositories;

namespace MetalReleaseTracker.Tests.Repositories
{
    public class BandRepositoryTests : IAsyncLifetime
    {
        private MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly BandRepository _repository;

        public BandRepositoryTests()
        {
            var configuration = new MapperConfiguration(configuration =>
            {
                configuration.AddProfile<MappingProfile>();
            });

            _dbContext = TestDbContextFactory.CreateDbContext();
            _mapper = configuration.CreateMapper();
            _repository = new BandRepository(_dbContext, _mapper);
        }

        public async Task InitializeAsync()
        {
            await InitializeData(_dbContext);
        }

        public Task DisposeAsync()
        {
            TestDbContextFactory.ClearDatabase(_dbContext);

            return Task.CompletedTask;
        }

        protected async Task InitializeData(MetalReleaseTrackerDbContext context)
        {
            var bands = new[]
            {
                new BandEntity { Id = Guid.NewGuid(), Name = "Metallica" },
                new BandEntity { Id = Guid.NewGuid(), Name = "Iron Maiden" }
            };
            context.Bands.AddRange(bands);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllBands()
        {
            var result = await _repository.GetAll();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetById_ShouldReturnBand_WhenIdExists()
        {
            var band = await _repository.GetAll();
            var bandId = band.First().Id;

            var result = await _repository.GetById(bandId);

            Assert.NotNull(result);
            Assert.Equal(bandId, result.Id);
        }

        [Fact]
        public async Task GetByName_ShouldReturnBand_WhenIdExists()
        {
            var band = await _repository.GetAll();
            var existingBand = band.First();

            var result = await _repository.GetByName(existingBand.Name);

            Assert.NotNull(result);
            Assert.Equal(existingBand.Name, result.Name);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var result = await _repository.GetById(Guid.NewGuid());

            Assert.Null(result);
        }


        [Fact]
        public async Task Add_ShouldAddBand()
        {
            var band = new Band
            {
                Id = Guid.NewGuid(),
                Name = "Iron Maiden"
            };

            await _repository.Add(band);

            var result = await _dbContext.Bands.FindAsync(band.Id);

            Assert.NotNull(result);
            Assert.Equal(band.Name, result.Name);
        }

        [Fact]
        public async Task Add_ShouldNotAddBand_WhenBandIsInvalid()
        {
            var band = new Band
            {
                Name = ""
            };

            await _repository.Add(band);

            var result = await _dbContext.Bands.FindAsync(band.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ShouldUpdateBand()
        {
            var bandEntity = _dbContext.Bands.First();
            var band = _mapper.Map<Band>(bandEntity);

            band.Name = "Updated Band";

            _dbContext.Entry(bandEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var result = await _repository.Update(band);

            var updatedEntity = await _dbContext.Bands.FindAsync(band.Id);

            Assert.True(result);
            Assert.NotNull(updatedEntity);
            Assert.Equal("Updated Band", updatedEntity.Name);
        }

        [Fact]
        public async Task Update_ShouldReturnFalse_WhenBandDoesNotExist()
        {
            var band = new Band
            {
                Id = Guid.NewGuid(),
                Name = "Non-existent Band"
            };

            var result = await _repository.Update(band);

            Assert.False(result);
        }

        [Fact]
        public async Task Delete_ShouldRemoveBand()
        {
            var bandEntity = _dbContext.Bands.First();
            var result = await _repository.Delete(bandEntity.Id);

            var deletedEntity = await _dbContext.Bands.FindAsync(bandEntity.Id);

            Assert.True(result);
            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenBandDoesNotExist()
        {
            var result = await _repository.Delete(Guid.NewGuid());

            Assert.False(result);
        }
    }
}