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
    public class BandServiceTest : IntegrationTestBase
    {
        private readonly BandRepository _bandRepository;
        private readonly ValidationService _validationService;
        private readonly BandService _bandService;

        public BandServiceTest()
        {
            _bandRepository = new BandRepository(DbContext, Mapper);
            _validationService = new ValidationService(new List<IValidator>
            {
                new BandValidator(),
                new GuidValidator()
            });
            _bandService = new BandService(_bandRepository, _validationService);
        }

        protected override void InitializeData(MetalReleaseTrackerDbContext context)
        {
            var bands = new[]
            {
                new BandEntity 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Metallica" 
                },
                new BandEntity 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Iron Maiden" 
                }
            };
            context.Bands.AddRange(bands);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetBandById_ShouldReturnBand_WhenIdExists()
        {
            var existingBand = await DbContext.Bands.FirstAsync();
            var result = await _bandService.GetBandById(existingBand.Id);

            Assert.NotNull(result);
            Assert.Equal(existingBand.Name, result.Name);
        }

        [Fact]
        public async Task GetBandById_ShouldThrowEntityNotFoundException_WhenIdDoesNotExist()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _bandService.GetBandById(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAllBands_ShouldReturnAllBands()
        {
            var result = await _bandService.GetAllBands();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task AddBand_ShouldAddBand()
        {
            var newBand = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Slayer" 
            };
            await _bandService.AddBand(newBand);

            var result = await DbContext.Bands.FindAsync(newBand.Id);

            Assert.NotNull(result);
            Assert.Equal(newBand.Name, result.Name);
        }

        [Fact]
        public async Task AddBand_ShouldThrowValidationException_WhenBandIsInvalid()
        {
            var band = new Band
            {
                Name = ""
            };

            await Assert.ThrowsAsync<ValidationException>(() => _bandService.AddBand(band));
        }

        [Fact]
        public async Task UpdateBand_ShouldUpdateBand()
        {
            var existingBand = await DbContext.Bands.FirstAsync();
            var updatedBand = Mapper.Map<Band>(existingBand);
            updatedBand.Name = "Updated Band Name";
            var result = await _bandService.UpdateBand(updatedBand);

            var retrievedBand = await DbContext.Bands.FindAsync(existingBand.Id);

            Assert.True(result);
            Assert.NotNull(retrievedBand);
            Assert.Equal(updatedBand.Name, retrievedBand.Name);
        }

        [Fact]
        public async Task UpdateBand_ShouldThrowEntityNotFoundException_WhenBandDoesNotExist()
        {
            var band = new Band
            {
                Id = Guid.NewGuid(),
                Name = "Non-existent Band"
            };

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _bandService.UpdateBand(band));
        }

        [Fact]
        public async Task DeleteBand_ShouldRemoveBand()
        {
            var band = await DbContext.Bands.FirstAsync();
            var result = await _bandService.DeleteBand(band.Id);

            var deletedBand = await DbContext.Bands.FindAsync(band.Id);

            Assert.True(result);
            Assert.Null(deletedBand);
        }

        [Fact]
        public async Task DeleteBand_ShouldThrowEntityNotFoundException_WhenBandDoesNotExist()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _bandService.DeleteBand(Guid.NewGuid()));
        }
    }
}
