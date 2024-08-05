using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;
using MetalReleaseTracker.Core.Validators;
using Moq;

namespace MetalReleaseTracker.Tests.Services
{
    public class BandServiceTest 
    {
        private readonly Mock<IBandRepository> _bandRepository;
        private readonly ValidationService _validationService;
        private readonly BandService _bandService;

        public BandServiceTest()
        {
            _bandRepository = new Mock<IBandRepository>();
            _validationService = new ValidationService(new List<IValidator>
            {
                new BandValidator(),
                new GuidValidator()
            });
            _bandService = new BandService(_bandRepository.Object, _validationService);
        }

        [Fact]
        public async Task GetBandById_ShouldReturnBand_WhenIdExists()
        {
            var bandId = Guid.NewGuid();
            var band = new Band 
            {
                Id = bandId, 
                Name = "Metallica" 
            };

            _bandRepository.Setup(repo => repo.GetById(bandId)).ReturnsAsync(band);

            var result = await _bandService.GetBandById(bandId);

            Assert.NotNull(result);
            Assert.Equal(bandId, result.Id);
            _bandRepository.Verify(repo => repo.GetById(bandId), Times.Once);
        }

        [Fact]
        public async Task GetBandById_ShouldThrowEntityNotFoundException_WhenIdDoesNotExist()
        {
            var bandId = Guid.NewGuid();
            _bandRepository.Setup(repo => repo.GetById(bandId)).ReturnsAsync((Band)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _bandService.GetBandById(bandId));
            _bandRepository.Verify(repo => repo.GetById(bandId), Times.Once);
        }

        [Fact]
        public async Task GetAllBands_ShouldReturnAllBands()
        {
            var bands = new List<Band>
            {
                new Band 
                { 
                    Id = Guid.NewGuid(), 
                    Name = "Metallica" 
                },

                new Band 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Iron Maiden" 
                }
            };

            _bandRepository.Setup(repo => repo.GetAll()).ReturnsAsync(bands);

            var result = await _bandService.GetAllBands();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _bandRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task AddBand_ShouldAddBand()
        {
            var newBand = new Band 
            { 
                Id = Guid.NewGuid(), 
                Name = "Slayer" 
            };

            _bandRepository.Setup(repo => repo.Add(newBand)).Returns(Task.CompletedTask);

            await _bandService.AddBand(newBand);

            _bandRepository.Verify(repo => repo.Add(newBand), Times.Once);
        }

        [Fact]
        public async Task AddBand_ShouldThrowValidationException_WhenBandIsInvalid()
        {
            var band = new Band
            {
                Name = ""
            };

            await Assert.ThrowsAsync<ValidationException>(() => _bandService.AddBand(band));
            _bandRepository.Verify(repo => repo.Add(It.IsAny<Band>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBand_ShouldUpdateBand()
        {
            var existingBand = new Band
            {
                Id = Guid.NewGuid(),
                Name = "Existing Band"
            };

            var updatedBand = new Band
            {
                Id = existingBand.Id,
                Name = "Updated Band Name"
            };

            _bandRepository.Setup(repo => repo.GetById(existingBand.Id)).ReturnsAsync(existingBand);
            _bandRepository.Setup(repo => repo.Update(updatedBand)).ReturnsAsync(true);

            var result = await _bandService.UpdateBand(updatedBand);

            Assert.True(result);
            _bandRepository.Verify(repo => repo.GetById(existingBand.Id), Times.Once);
            _bandRepository.Verify(repo => repo.Update(updatedBand), Times.Once);
        }

        [Fact]
        public async Task UpdateBand_ShouldThrowEntityNotFoundException_WhenBandDoesNotExist()
        {
            var band = new Band
            {
                Id = Guid.NewGuid(),
                Name = "Non-existent Band"
            };

            _bandRepository.Setup(repo => repo.GetById(band.Id)).ReturnsAsync((Band)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _bandService.UpdateBand(band));
            _bandRepository.Verify(repo => repo.GetById(band.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteBand_ShouldRemoveBand()
        {
            var existingBand = new Band
            {
                Id = Guid.NewGuid(),
                Name = "Existing Band"
            };

            _bandRepository.Setup(repo => repo.GetById(existingBand.Id)).ReturnsAsync(existingBand);
            _bandRepository.Setup(repo => repo.Delete(existingBand.Id)).ReturnsAsync(true);

            var result = await _bandService.DeleteBand(existingBand.Id);

            Assert.True(result);
            _bandRepository.Verify(repo => repo.GetById(existingBand.Id), Times.Once);
            _bandRepository.Verify(repo => repo.Delete(existingBand.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteBand_ShouldThrowEntityNotFoundException_WhenBandDoesNotExist()
        {
            var bandId = Guid.NewGuid();
            _bandRepository.Setup(repo => repo.GetById(bandId)).ReturnsAsync((Band)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _bandService.DeleteBand(bandId));
            _bandRepository.Verify(repo => repo.GetById(bandId), Times.Once);
            _bandRepository.Verify(repo => repo.Delete(bandId), Times.Never);
        }
    }
}
