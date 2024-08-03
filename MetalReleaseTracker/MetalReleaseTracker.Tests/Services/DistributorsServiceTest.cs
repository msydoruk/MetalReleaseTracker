using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Services;
using MetalReleaseTracker.Core.Validators;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Repositories;
using MetalReleaseTracker.Tests.Base;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Tests.Services
{
    public class DistributorsServiceTest : IntegrationTestBase
    {
        private readonly DistributorsRepository _distributorsRepository;
        private readonly ValidationService _validationService;
        private readonly DistributorsService _distributorsService;

        public DistributorsServiceTest()
        {
            _distributorsRepository = new DistributorsRepository(DbContext, Mapper);
            _validationService = new ValidationService(new List<IValidator>
            {
                new DistributorValidator(),
                new GuidValidator()
            });
            _distributorsService = new DistributorsService(_distributorsRepository, _validationService);
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
                    Name = "Sony Music",
                    ParsingUrl = "https://example.com/warner"
                }
            };
            context.Distributors.AddRange(distributors);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetDistributorById_ShouldReturnDistributor_WhenIdExists()
        {
            var existingDistributor = await DbContext.Distributors.FirstAsync();
            var result = await _distributorsService.GetDistributorById(existingDistributor.Id);

            Assert.NotNull(result);
            Assert.Equal(existingDistributor.Name, result.Name);
        }

        [Fact]
        public async Task GetDistributorById_ShouldThrowEntityNotFoundException_WhenIdDoesNotExist()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _distributorsService.GetDistributorById(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAllDistributors_ShouldReturnAllDistributors()
        {
            var result = await _distributorsService.GetAllDistributors();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddDistributor_ShouldAddDistributor()
        {
            var newDistributor = new Distributor 
            { 
                Id = Guid.NewGuid(), 
                Name = "Warner Music",
                ParsingUrl = "https://example.com/new"
            };
            await _distributorsService.AddDistributor(newDistributor);

            var result = await DbContext.Distributors.FindAsync(newDistributor.Id);

            Assert.NotNull(result);
            Assert.Equal(newDistributor.Name, result.Name);
        }

        [Fact]
        public async Task AddDistributor_ShouldThrowValidationException_WhenDistributorIsInvalid()
        {
            var distributor = new Distributor
            {
                Name = "",
                ParsingUrl = ""
            };

            await Assert.ThrowsAsync<ValidationException>(() => _distributorsService.AddDistributor(distributor));
        }

        [Fact]
        public async Task UpdateDistributor_ShouldUpdateDistributor()
        {
            var existingDistributor = await DbContext.Distributors.FirstAsync();
            var updatedDistributor = Mapper.Map<Distributor>(existingDistributor);
            updatedDistributor.Name = "Updated Distributor Name";
            var result = await _distributorsService.UpdateDistributor(updatedDistributor);

            var retrievedDistributor = await DbContext.Distributors.FindAsync(existingDistributor.Id);

            Assert.True(result);
            Assert.NotNull(retrievedDistributor);
            Assert.Equal(updatedDistributor.Name, retrievedDistributor.Name);
        }

        [Fact]
        public async Task UpdateDistributor_ShouldThrowEntityNotFoundException_WhenDistributorDoesNotExist()
        {
            var distributor = new Distributor
            {
                Id = Guid.NewGuid(),
                Name = "Non-existent Distributor",
                ParsingUrl = "https://example.com/test"
            };

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _distributorsService.UpdateDistributor(distributor));
        }

        [Fact]
        public async Task DeleteDistributor_ShouldRemoveDistributor()
        {
            var distributor = await DbContext.Distributors.FirstAsync();
            var result = await _distributorsService.DeleteDistributor(distributor.Id);

            var deletedDistributor = await DbContext.Distributors.FindAsync(distributor.Id);

            Assert.True(result);
            Assert.Null(deletedDistributor);
        }

        [Fact]
        public async Task DeleteDistributor_ShouldThrowEntityNotFoundException_WhenDistributorDoesNotExist()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _distributorsService.DeleteDistributor(Guid.NewGuid()));
        }
    }
}
