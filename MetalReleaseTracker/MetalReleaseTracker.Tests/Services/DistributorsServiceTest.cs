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
        public async Task GetDistributorById_WhenIdExists_ShouldReturnDistributor()
        {
            var distributorId = Guid.NewGuid();
            var distributor = CreateSampleDistributor(distributorId);
            _distributorsRepository.Setup(repository => repository.GetById(distributorId)).ReturnsAsync(distributor);

            var result = await _distributorsService.GetDistributorById(distributorId);

            Assert.NotNull(result);
            Assert.Equal(distributorId, result.Id);
            _distributorsRepository.Verify(repository => repository.GetById(distributorId), Times.Once);
        }

        [Fact]
        public async Task GetDistributorById_WhenIdDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var distributorId = Guid.NewGuid();
            _distributorsRepository.Setup(repository => repository.GetById(distributorId)).ReturnsAsync((Distributor)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _distributorsService.GetDistributorById(distributorId));
            _distributorsRepository.Verify(repository => repository.GetById(distributorId), Times.Once);
        }

        [Fact]
        public async Task GetAllDistributors_ShouldReturnAllDistributors()
        {
            var distributors = CreateSampleDistributors();
            _distributorsRepository.Setup(repository => repository.GetAll()).ReturnsAsync(distributors);

            var result = await _distributorsService.GetAllDistributors();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _distributorsRepository.Verify(repository => repository.GetAll(), Times.Once);
        }

        [Fact]
        public async Task AddDistributor_ShouldAddDistributor()
        {
            var newDistributor = CreateSampleDistributor(name: "Warner Music", parsingUrl: "https://example.com/warner");
            _distributorsRepository.Setup(repository => repository.Add(newDistributor)).Returns(Task.CompletedTask);

            await _distributorsService.AddDistributor(newDistributor);

            _distributorsRepository.Verify(repository => repository.Add(newDistributor), Times.Once);
        }

        [Fact]
        public async Task AddDistributor_WhenDistributorIsInvalid_ShouldThrowValidationException()
        {
            var distributor = new Distributor
            {
                Name = "",
                ParsingUrl = ""
            };

            await Assert.ThrowsAsync<ValidationException>(() => _distributorsService.AddDistributor(distributor));
            _distributorsRepository.Verify(repository => repository.Add(It.IsAny<Distributor>()), Times.Never);
        }

        [Fact]
        public async Task UpdateDistributor_ShouldUpdateDistributor()
        {
            var existingDistributor = CreateSampleDistributor(name: "Existing Distributor", parsingUrl: "https://example.com/existing");
            var updatedDistributor = CreateSampleDistributor(existingDistributor.Id, name: "Updated Distributor Name", parsingUrl: existingDistributor.ParsingUrl);

            _distributorsRepository.Setup(repository => repository.GetById(existingDistributor.Id)).ReturnsAsync(existingDistributor);
            _distributorsRepository.Setup(repository => repository.Update(updatedDistributor)).ReturnsAsync(true);

            var result = await _distributorsService.UpdateDistributor(updatedDistributor);

            Assert.True(result);
            _distributorsRepository.Verify(repository => repository.GetById(existingDistributor.Id), Times.Once);
            _distributorsRepository.Verify(repository => repository.Update(updatedDistributor), Times.Once);
        }

        [Fact]
        public async Task UpdateDistributor_WhenDistributorDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var distributor = CreateSampleDistributor(name: "Non-existent Distributor", parsingUrl: "https://example.com/nonexistent");
            _distributorsRepository.Setup(repository => repository.GetById(distributor.Id)).ReturnsAsync((Distributor)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _distributorsService.UpdateDistributor(distributor));
            _distributorsRepository.Verify(repository => repository.GetById(distributor.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteDistributor_ShouldRemoveDistributor()
        {
            var existingDistributor = CreateSampleDistributor(name: "Existing Distributor", parsingUrl: "https://example.com/existing");
            _distributorsRepository.Setup(repository => repository.GetById(existingDistributor.Id)).ReturnsAsync(existingDistributor);
            _distributorsRepository.Setup(repository => repository.Delete(existingDistributor.Id)).ReturnsAsync(true);

            var result = await _distributorsService.DeleteDistributor(existingDistributor.Id);

            Assert.True(result);
            _distributorsRepository.Verify(repository => repository.GetById(existingDistributor.Id), Times.Once);
            _distributorsRepository.Verify(repository => repository.Delete(existingDistributor.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteDistributor_WhenDistributorDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var distributorId = Guid.NewGuid();
            _distributorsRepository.Setup(repository => repository.GetById(distributorId)).ReturnsAsync((Distributor)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _distributorsService.DeleteDistributor(distributorId));
            _distributorsRepository.Verify(repository => repository.GetById(distributorId), Times.Once);
            _distributorsRepository.Verify(repository => repository.Delete(distributorId), Times.Never);
        }

        private Distributor CreateSampleDistributor(Guid? id = null, string name = "Test Distributor", string parsingUrl = "https://example.com")
        {
            return new Distributor
            {
                Id = id ?? Guid.NewGuid(),
                Name = name,
                ParsingUrl = parsingUrl
            };
        }

        private List<Distributor> CreateSampleDistributors()
        {
            return new List<Distributor>
            {
                CreateSampleDistributor(name: "Universal Music", parsingUrl: "https://example.com/universal"),
                CreateSampleDistributor(name: "Sony Music", parsingUrl: "https://example.com/sony")
            };
        }
    }
}
