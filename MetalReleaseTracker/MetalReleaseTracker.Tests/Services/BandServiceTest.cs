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
        public async Task GetBandById_WhenIdExists_ShouldReturnBand()
        {
            var bandId = Guid.NewGuid();
            var band = CreateSampleBand(bandId);

            _bandRepository.Setup(repository => repository.GetById(bandId)).ReturnsAsync(band);

            var result = await _bandService.GetBandById(bandId);

            Assert.NotNull(result);
            Assert.Equal(bandId, result.Id);
            _bandRepository.Verify(repository => repository.GetById(bandId), Times.Once);
        }

        [Fact]
        public async Task GetBandById_WhenIdDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var bandId = Guid.NewGuid();
            _bandRepository.Setup(repository => repository.GetById(bandId)).ReturnsAsync((Band)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _bandService.GetBandById(bandId));
            _bandRepository.Verify(repository => repository.GetById(bandId), Times.Once);
        }

        [Fact]
        public async Task GetAllBands_ShouldReturnAllBands()
        {
            var bands = CreateSampleBands();
            _bandRepository.Setup(repository => repository.GetAll()).ReturnsAsync(bands);

            var result = await _bandService.GetAllBands();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _bandRepository.Verify(repository => repository.GetAll(), Times.Once);
        }

        [Fact]
        public async Task AddBand_ShouldAddBand()
        {
            var newBand = CreateSampleBand(name: "Slayer");
            _bandRepository.Setup(repository => repository.Add(newBand)).Returns(Task.CompletedTask);

            await _bandService.AddBand(newBand);

            _bandRepository.Verify(repository => repository.Add(newBand), Times.Once);
        }

        [Fact]
        public async Task AddBand_WhenBandIsInvalid_ShouldThrowValidationException()
        {
            var band = new Band
            {
                Name = ""
            };

            await Assert.ThrowsAsync<ValidationException>(() => _bandService.AddBand(band));
            _bandRepository.Verify(repository => repository.Add(It.IsAny<Band>()), Times.Never);
        }

        [Fact]
        public async Task UpdateBand_ShouldUpdateBand()
        {
            var existingBand = CreateSampleBand();
            var updatedBand = CreateSampleBand(existingBand.Id, name: "Updated Band Name");

            _bandRepository.Setup(repository => repository.GetById(existingBand.Id)).ReturnsAsync(existingBand);
            _bandRepository.Setup(repository => repository.Update(updatedBand)).ReturnsAsync(true);

            var result = await _bandService.UpdateBand(updatedBand);

            Assert.True(result);
            _bandRepository.Verify(repository => repository.GetById(existingBand.Id), Times.Once);
            _bandRepository.Verify(repository => repository.Update(updatedBand), Times.Once);
        }

        [Fact]
        public async Task UpdateBand_WhenBandDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var band = CreateSampleBand(name: "Non-existent Band");
            _bandRepository.Setup(repository => repository.GetById(band.Id)).ReturnsAsync((Band)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _bandService.UpdateBand(band));
            _bandRepository.Verify(repository => repository.GetById(band.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteBand_ShouldRemoveBand()
        {
            var existingBand = CreateSampleBand();
            _bandRepository.Setup(repository => repository.GetById(existingBand.Id)).ReturnsAsync(existingBand);
            _bandRepository.Setup(repository => repository.Delete(existingBand.Id)).ReturnsAsync(true);

            var result = await _bandService.DeleteBand(existingBand.Id);

            Assert.True(result);
            _bandRepository.Verify(repository => repository.GetById(existingBand.Id), Times.Once);
            _bandRepository.Verify(repository => repository.Delete(existingBand.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteBand_WhenBandDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var bandId = Guid.NewGuid();
            _bandRepository.Setup(repository => repository.GetById(bandId)).ReturnsAsync((Band)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _bandService.DeleteBand(bandId));
            _bandRepository.Verify(repository => repository.GetById(bandId), Times.Once);
            _bandRepository.Verify(repository => repository.Delete(bandId), Times.Never);
        }

        private Band CreateSampleBand(Guid? id = null, string name = "Test Band")
        {
            return new Band
            {
                Id = id ?? Guid.NewGuid(),
                Name = name
            };
        }

        private List<Band> CreateSampleBands()
        {
            return new List<Band>
            {
                CreateSampleBand(name: "Metallica"),
                CreateSampleBand(name: "Iron Maiden")
            };
        }
    }
}
