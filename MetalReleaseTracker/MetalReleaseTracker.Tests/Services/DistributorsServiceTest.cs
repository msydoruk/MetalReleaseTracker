using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;
using MetalReleaseTracker.Core.Validators;
using Moq;

namespace MetalReleaseTracker.Tests.Services
{
    public class DistributorsServiceTest
    {
        private readonly Mock<IDistributorsRepository> _distributorsRepository;
        private readonly ValidationService _validationService;
        private readonly DistributorsService _distributorsService;

        public DistributorsServiceTest()
        {
            _distributorsRepository = new Mock<IDistributorsRepository>();
            _validationService = new ValidationService(new List<IValidator>
            {
                new DistributorValidator(),
                new GuidValidator()
            });
            _distributorsService = new DistributorsService(_distributorsRepository.Object, _validationService);
        }

        [Fact]
        public async Task GetDistributorById_ShouldReturnDistributor_WhenIdExists()
        {
            var distributorId = Guid.NewGuid();
            var distributor = new Distributor 
            { 
                Id = distributorId, 
                Name = "Warner Music",
                ParsingUrl = "https://example.com/new"
            };

            _distributorsRepository.Setup(repo => repo.GetById(distributorId)).ReturnsAsync(distributor);

            var result = await _distributorsService.GetDistributorById(distributorId);

            Assert.NotNull(result);
            Assert.Equal(distributorId, result.Id);
            _distributorsRepository.Verify(repo => repo.GetById(distributorId), Times.Once);
        }

        [Fact]
        public async Task GetDistributorById_ShouldThrowEntityNotFoundException_WhenIdDoesNotExist()
        {
            var distributorId = Guid.NewGuid();
            _distributorsRepository.Setup(repo => repo.GetById(distributorId)).ReturnsAsync((Distributor)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _distributorsService.GetDistributorById(distributorId));
            _distributorsRepository.Verify(repo => repo.GetById(distributorId), Times.Once);
        }

        [Fact]
        public async Task GetAllDistributors_ShouldReturnAllDistributors()
        {
            var distributors = new List<Distributor>
            {
                new Distributor 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Universal Music",
                    ParsingUrl = "https://example.com/universal"
                },

                new Distributor 
                { 
                    Id = Guid.NewGuid(),
                    Name = "Sony Music",
                    ParsingUrl = "https://example.com/warner"
                }
            };

            _distributorsRepository.Setup(repo => repo.GetAll()).ReturnsAsync(distributors);

            var result = await _distributorsService.GetAllDistributors();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _distributorsRepository.Verify(repo => repo.GetAll(), Times.Once);
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

            _distributorsRepository.Setup(repo => repo.Add(newDistributor)).Returns(Task.CompletedTask);

            await _distributorsService.AddDistributor(newDistributor);

            _distributorsRepository.Verify(repo => repo.Add(newDistributor), Times.Once);
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
            _distributorsRepository.Verify(repo => repo.Add(It.IsAny<Distributor>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDistributor_ShouldUpdateDistributor()
        {
            var existingDistributor = new Distributor
            {
                Id = Guid.NewGuid(),
                Name = "Existing Distributor",
                ParsingUrl = "https://example.com/universal"
            };

            var updatedDistributor = new Distributor
            {
                Id = existingDistributor.Id,
                Name = "Updated Distributor Name",
                ParsingUrl = existingDistributor.ParsingUrl
            };

            _distributorsRepository.Setup(repo => repo.GetById(existingDistributor.Id)).ReturnsAsync(existingDistributor);
            _distributorsRepository.Setup(repo => repo.Update(updatedDistributor)).ReturnsAsync(true);

            var result = await _distributorsService.UpdateDistributor(updatedDistributor);

            Assert.True(result);
            _distributorsRepository.Verify(repo => repo.GetById(existingDistributor.Id), Times.Once);
            _distributorsRepository.Verify(repo => repo.Update(updatedDistributor), Times.Once);
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

            _distributorsRepository.Setup(repo => repo.GetById(distributor.Id)).ReturnsAsync((Distributor)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _distributorsService.UpdateDistributor(distributor));
            _distributorsRepository.Verify(repo => repo.GetById(distributor.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteDistributor_ShouldRemoveDistributor()
        {
            var existingDistributor = new Distributor
            {
                Id = Guid.NewGuid(),
                Name = "Existing Distributor",
                ParsingUrl = "https://example.com/test"
            };

            _distributorsRepository.Setup(repo => repo.GetById(existingDistributor.Id)).ReturnsAsync(existingDistributor);
            _distributorsRepository.Setup(repo => repo.Delete(existingDistributor.Id)).ReturnsAsync(true);

            var result = await _distributorsService.DeleteDistributor(existingDistributor.Id);

            Assert.True(result);
            _distributorsRepository.Verify(repo => repo.GetById(existingDistributor.Id), Times.Once);
            _distributorsRepository.Verify(repo => repo.Delete(existingDistributor.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteDistributor_ShouldThrowEntityNotFoundException_WhenDistributorDoesNotExist()
        {
            var distributorId = Guid.NewGuid();
            _distributorsRepository.Setup(repo => repo.GetById(distributorId)).ReturnsAsync((Distributor)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _distributorsService.DeleteDistributor(distributorId));
            _distributorsRepository.Verify(repo => repo.GetById(distributorId), Times.Once);
            _distributorsRepository.Verify(repo => repo.Delete(distributorId), Times.Never);
        }
    }
}
