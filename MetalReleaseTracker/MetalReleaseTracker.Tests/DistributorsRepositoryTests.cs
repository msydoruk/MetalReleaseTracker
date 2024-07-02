using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Repositories;

namespace MetalReleaseTracker.Tests
{
    public class DistributorsRepositoryTests : IntegrationTestBase
    {

        private readonly DistributorsRepository _repository;

        public DistributorsRepositoryTests()
        {
            _repository = new DistributorsRepository(DbContext, Mapper);
        }

        protected override void InitializeData(MetalReleaseTrackerDbContext context)
        {
            var distributors = new[]
            {
                new DistributorEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Universal Music",
                    ParsingUrl = "https://example.com/universal"
                },
                new DistributorEntity
                {
                    Id = Guid.NewGuid(),
                    Name = "Warner Music",
                    ParsingUrl = "https://example.com/warner"
                }
            };
            context.Distributors.AddRange(distributors);
            context.SaveChanges();
        }

        [Fact]
        public async Task Add_ShouldAddDistributor()
        {
            var distributor = new Distributor
            {
                Id = Guid.NewGuid(),
                Name = "Warner Music",
                ParsingUrl = "https://example.com/new"
            };

            await _repository.Add(distributor);

            var result = await DbContext.Distributors.FindAsync(distributor.Id);

            Assert.NotNull(result);
            Assert.Equal(distributor.Name, result.Name);
            Assert.Equal(distributor.ParsingUrl, result.ParsingUrl);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllDistributors()
        {
            var result = await _repository.GetAll();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetById_ShouldReturnDistributor_WhenIdExists()
        {
            var distributorEntity = DbContext.Distributors.First();
            var result = await _repository.GetById(distributorEntity.Id);

            Assert.NotNull(result);
            Assert.Equal(distributorEntity.Id, result.Id);
        }

        [Fact]
        public async Task Update_ShouldUpdateDistributor()
        {
            var distributorEntity = DbContext.Distributors.First();
            var distributor = Mapper.Map<Distributor>(distributorEntity);

            distributor.Name = "Updated Distributor";
            distributor.ParsingUrl = "https://example.com/updated";

            DbContext.Entry(distributorEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            await _repository.Update(distributor);

            var result = await DbContext.Distributors.FindAsync(distributor.Id);

            Assert.NotNull(result);
            Assert.Equal("Updated Distributor", result.Name);
            Assert.Equal("https://example.com/updated", result.ParsingUrl);
        }

        [Fact]
        public async Task Delete_ShouldRemoveDistributor()
        {
            var distributorEntity = DbContext.Distributors.First();
            await _repository.Delete(distributorEntity.Id);

            var result = await DbContext.Distributors.FindAsync(distributorEntity.Id);

            Assert.Null(result);
        }
    }
}
