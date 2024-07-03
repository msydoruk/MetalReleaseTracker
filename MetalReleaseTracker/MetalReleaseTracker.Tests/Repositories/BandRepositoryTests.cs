﻿using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Repositories;
using MetalReleaseTracker.Tests.Base;

namespace MetalReleaseTracker.Tests.Repositories
{
    public class BandRepositoryTests : IntegrationTestBase
    {
        private readonly BandRepository _repository;

        public BandRepositoryTests()
        {
            _repository = new BandRepository(DbContext, Mapper);
        }

        protected override void InitializeData(MetalReleaseTrackerDbContext context)
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

            var result = await DbContext.Bands.FindAsync(band.Id);

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

            var result = await DbContext.Bands.FindAsync(band.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ShouldUpdateBand()
        {
            var bandEntity = DbContext.Bands.First();
            var band = Mapper.Map<Band>(bandEntity);

            band.Name = "Updated Band";

            DbContext.Entry(bandEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var result = await _repository.Update(band);

            var updatedEntity = await DbContext.Bands.FindAsync(band.Id);

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
            var bandEntity = DbContext.Bands.First();
            var result = await _repository.Delete(bandEntity.Id);

            var deletedEntity = await DbContext.Bands.FindAsync(bandEntity.Id);

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