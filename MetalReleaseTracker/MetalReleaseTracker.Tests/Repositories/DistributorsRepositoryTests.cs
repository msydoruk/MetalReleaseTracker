using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Repositories;
using MetalReleaseTracker.Tests.Base;

namespace MetalReleaseTracker.Tests.Repositories
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
        public async Task Add_ShouldNotAddDistributor_WhenDistributorIsInvalid()
        {
            var distributor = new Distributor
            {
                Name = "",
                ParsingUrl = "https://example.com/new"
            };

            await _repository.Add(distributor);

            var result = await DbContext.Distributors.FindAsync(distributor.Id);

            Assert.Null(result);
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
        public async Task GetById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var result = await _repository.GetById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ShouldUpdateDistributor()
        {
            var distributorEntity = DbContext.Distributors.First();
            var distributor = Mapper.Map<Distributor>(distributorEntity);

            distributor.Name = "Updated Distributor";
            distributor.ParsingUrl = "https://example.com/updated";

            DbContext.Entry(distributorEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var result = await _repository.Update(distributor);

            var updatedEntity = await DbContext.Distributors.FindAsync(distributor.Id);

            Assert.True(result);
            Assert.NotNull(updatedEntity);
            Assert.Equal("Updated Distributor", updatedEntity.Name);
            Assert.Equal("https://example.com/updated", updatedEntity.ParsingUrl);
        }

        [Fact]
        public async Task Update_ShouldReturnFalse_WhenDistributorDoesNotExist()
        {
            var distributor = new Distributor
            {
                Id = Guid.NewGuid(),
                Name = "Test1",
            };

            var result = await _repository.Update(distributor);

            Assert.False(result);
        }

        [Fact]
        public async Task Delete_ShouldRemoveDistributor()
        {
            var distributorEntity = DbContext.Distributors.First();
            var result = await _repository.Delete(distributorEntity.Id);

            var deletedEntity = await DbContext.Distributors.FindAsync(distributorEntity.Id);

            Assert.True(result);
            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenDistributorDoesNotExist()
        {
            var result = await _repository.Delete(Guid.NewGuid());

            Assert.False(result);
        }
    }
}
