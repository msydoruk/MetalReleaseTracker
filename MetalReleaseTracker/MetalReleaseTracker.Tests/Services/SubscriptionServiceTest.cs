using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Exceptions;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;
using MetalReleaseTracker.Core.Validators;
using Moq;

namespace MetalReleaseTracker.Tests.Services
{
    public class SubscriptionServiceTest
    {
        private readonly Mock<ISubscriptionRepository> _subscriptionRepository;
        private readonly ValidationService _validationService;
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionServiceTest()
        {
            _subscriptionRepository = new Mock<ISubscriptionRepository>();
            _validationService = new ValidationService(new List<IValidator>
            {
                new SubscriptionValidator(),
                new GuidValidator()
            });
            _subscriptionService = new SubscriptionService(_subscriptionRepository.Object, _validationService);
        }

        [Fact]
        public async Task GetSubscriptionById_WhenIdExists_ShouldReturnSubscription()
        {
            var subscriptionId = Guid.NewGuid();
            var subscription = CreateSampleSubscription(subscriptionId, email: "test3@example.com");
            _subscriptionRepository.Setup(repository => repository.GetById(subscriptionId)).ReturnsAsync(subscription);

            var result = await _subscriptionService.GetSubscriptionById(subscriptionId);

            Assert.NotNull(result);
            Assert.Equal(subscriptionId, result.Id);
            _subscriptionRepository.Verify(repository => repository.GetById(subscriptionId), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionById_WhenIdDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var subscriptionId = Guid.NewGuid();
            _subscriptionRepository.Setup(repository => repository.GetById(subscriptionId)).ReturnsAsync((Subscription)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionService.GetSubscriptionById(subscriptionId));
            _subscriptionRepository.Verify(repository => repository.GetById(subscriptionId), Times.Once);
        }

        [Fact]
        public async Task GetAllSubscriptions_ShouldReturnAllSubscriptions()
        {
            var subscriptions = CreateSampleSubscriptions();
            _subscriptionRepository.Setup(repository => repository.GetAll()).ReturnsAsync(subscriptions);

            var result = await _subscriptionService.GetAllSubscriptions();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _subscriptionRepository.Verify(repository => repository.GetAll(), Times.Once);
        }

        [Fact]
        public async Task AddSubscription_ShouldAddSubscription()
        {
            var newSubscription = CreateSampleSubscription(email: "test3@example.com");
            _subscriptionRepository.Setup(repository => repository.Add(newSubscription)).Returns(Task.CompletedTask);

            await _subscriptionService.AddSubscription(newSubscription);

            _subscriptionRepository.Verify(repository => repository.Add(newSubscription), Times.Once);
        }

        [Fact]
        public async Task AddSubscription_WhenSubscriptionIsInvalid_ShouldThrowValidationException()
        {
            var subscription = CreateSampleSubscription(email: "");

            await Assert.ThrowsAsync<ValidationException>(() => _subscriptionService.AddSubscription(subscription));
            _subscriptionRepository.Verify(repository => repository.Add(It.IsAny<Subscription>()), Times.Never);
        }

        [Fact]
        public async Task UpdateSubscription_ShouldUpdateSubscription()
        {
            var existingSubscription = CreateSampleSubscription(email: "test1@example.com");
            var updatedSubscription = CreateSampleSubscription(existingSubscription.Id, email: "testUpdate@example.com");

            _subscriptionRepository.Setup(repository => repository.GetById(existingSubscription.Id)).ReturnsAsync(existingSubscription);
            _subscriptionRepository.Setup(repository => repository.Update(updatedSubscription)).ReturnsAsync(true);

            var result = await _subscriptionService.UpdateSubscription(updatedSubscription);

            Assert.True(result);
            _subscriptionRepository.Verify(repository => repository.GetById(existingSubscription.Id), Times.Once);
            _subscriptionRepository.Verify(repository => repository.Update(updatedSubscription), Times.Once);
        }

        [Fact]
        public async Task UpdateSubscription_WhenSubscriptionDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var subscription = CreateSampleSubscription(email: "invalid@gmail.com");
            _subscriptionRepository.Setup(repository => repository.GetById(subscription.Id)).ReturnsAsync((Subscription)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionService.UpdateSubscription(subscription));
            _subscriptionRepository.Verify(repository => repository.GetById(subscription.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteSubscription_ShouldRemoveSubscription()
        {
            var existingSubscription = CreateSampleSubscription(email: "test1@example.com");
            _subscriptionRepository.Setup(repository => repository.GetById(existingSubscription.Id)).ReturnsAsync(existingSubscription);
            _subscriptionRepository.Setup(repository => repository.Delete(existingSubscription.Id)).ReturnsAsync(true);

            var result = await _subscriptionService.DeleteSubscription(existingSubscription.Id);

            Assert.True(result);
            _subscriptionRepository.Verify(repository => repository.GetById(existingSubscription.Id), Times.Once);
            _subscriptionRepository.Verify(repository => repository.Delete(existingSubscription.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteSubscription_WhenSubscriptionDoesNotExist_ShouldThrowEntityNotFoundException()
        {
            var subscriptionId = Guid.NewGuid();
            _subscriptionRepository.Setup(repository => repository.GetById(subscriptionId)).ReturnsAsync((Subscription)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionService.DeleteSubscription(subscriptionId));
            _subscriptionRepository.Verify(repository => repository.GetById(subscriptionId), Times.Once);
            _subscriptionRepository.Verify(repository => repository.Delete(subscriptionId), Times.Never);
        }

        private Subscription CreateSampleSubscription(Guid? id = null, string email = "test@example.com", bool notifyForNewReleases = true)
        {
            return new Subscription
            {
                Id = id ?? Guid.NewGuid(),
                Email = email,
                NotifyForNewReleases = notifyForNewReleases
            };
        }

        private List<Subscription> CreateSampleSubscriptions()
        {
            return new List<Subscription>
            {
                CreateSampleSubscription(email: "test1@example.com", notifyForNewReleases: true),
                CreateSampleSubscription(email: "test2@example.com", notifyForNewReleases: false)
            };
        }
    }
}
