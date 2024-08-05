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
        public async Task GetSubscriptionById_ShouldReturnSubscription_WhenIdExists()
        {
            var subscriptionId = Guid.NewGuid();
            var subscription = new Subscription
            {
                Id = subscriptionId,
                Email = "test3@example.com",
                NotifyForNewReleases = true
            };

            _subscriptionRepository.Setup(repo => repo.GetById(subscriptionId)).ReturnsAsync(subscription);

            var result = await _subscriptionService.GetSubscriptionById(subscriptionId);

            Assert.NotNull(result);
            Assert.Equal(subscriptionId, result.Id);
            _subscriptionRepository.Verify(repo => repo.GetById(subscriptionId), Times.Once);
        }

        [Fact]
        public async Task GetSubscriptionById_ShouldThrowEntityNotFoundException_WhenIdDoesNotExist()
        {
            var subscriptionId = Guid.NewGuid();
            _subscriptionRepository.Setup(repo => repo.GetById(subscriptionId)).ReturnsAsync((Subscription)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionService.GetSubscriptionById(subscriptionId));
            _subscriptionRepository.Verify(repo => repo.GetById(subscriptionId), Times.Once);
        }

        [Fact]
        public async Task GetAllSubscriptions_ShouldReturnAllSubscriptions()
        {
            var subscriptions = new List<Subscription>
            {
                new Subscription
                {
                    Id = Guid.NewGuid(),
                    Email = "test1@example.com",
                    NotifyForNewReleases = true
                },

                new Subscription
                {
                    Id = Guid.NewGuid(),
                    Email = "test2@example.com",
                    NotifyForNewReleases = false
                }
            };

            _subscriptionRepository.Setup(repo => repo.GetAll()).ReturnsAsync(subscriptions);

            var result = await _subscriptionService.GetAllSubscriptions();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
            _subscriptionRepository.Verify(repo => repo.GetAll(), Times.Once);
        }

        [Fact]
        public async Task AddSubscription_ShouldAddSubscription()
        {
            var newSubscription = new Subscription 
            { 
                Id = Guid.NewGuid(), 
                Email = "test3@example.com",
                NotifyForNewReleases = true
            };

            _subscriptionRepository.Setup(repo => repo.Add(newSubscription)).Returns(Task.CompletedTask);

            await _subscriptionService.AddSubscription(newSubscription);

            _subscriptionRepository.Verify(repo => repo.Add(newSubscription), Times.Once);
        }

        [Fact]
        public async Task AddSubscription_ShouldThrowValidationException_WhenSubscriptionIsInvalid()
        {
            var subscription = new Subscription
            {
                Email =  ""
            };

            await Assert.ThrowsAsync<ValidationException>(() => _subscriptionService.AddSubscription(subscription));
            _subscriptionRepository.Verify(repo => repo.Add(It.IsAny<Subscription>()), Times.Never);
        }

        [Fact]
        public async Task UpdateSubscription_ShouldUpdateSubscription()
        {
            var existingSubscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Email = "test1@example.com",
                NotifyForNewReleases = true
            };

            var updatedSubscription = new Subscription
            {
                Id = existingSubscription.Id,
                Email = "testUpdate@example.com",
                NotifyForNewReleases = existingSubscription.NotifyForNewReleases
            };

            _subscriptionRepository.Setup(repo => repo.GetById(existingSubscription.Id)).ReturnsAsync(existingSubscription);
            _subscriptionRepository.Setup(repo => repo.Update(updatedSubscription)).ReturnsAsync(true);

            var result = await _subscriptionService.UpdateSubscription(updatedSubscription);

            Assert.True(result);
            _subscriptionRepository.Verify(repo => repo.GetById(existingSubscription.Id), Times.Once);
            _subscriptionRepository.Verify(repo => repo.Update(updatedSubscription), Times.Once);
        }

        [Fact]
        public async Task UpdateSubscription_ShouldThrowEntityNotFoundException_WhenSubscriptionDoesNotExist()
        {
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Email = "invalid@gmail.com",
                NotifyForNewReleases = false
            };

            _subscriptionRepository.Setup(repo => repo.GetById(subscription.Id)).ReturnsAsync((Subscription)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionService.UpdateSubscription(subscription));
            _subscriptionRepository.Verify(repo => repo.GetById(subscription.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteSubscription_ShouldRemoveSubscription()
        {
            var existingSubscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Email = "test1@example.com",
                NotifyForNewReleases = true
            };

            _subscriptionRepository.Setup(repo => repo.GetById(existingSubscription.Id)).ReturnsAsync(existingSubscription);
            _subscriptionRepository.Setup(repo => repo.Delete(existingSubscription.Id)).ReturnsAsync(true);

            var result = await _subscriptionService.DeleteSubscription(existingSubscription.Id);

            Assert.True(result);
            _subscriptionRepository.Verify(repo => repo.GetById(existingSubscription.Id), Times.Once);
            _subscriptionRepository.Verify(repo => repo.Delete(existingSubscription.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteSubscription_ShouldThrowEntityNotFoundException_WhenSubscriptionDoesNotExist()
        {
            var subscriptionId = Guid.NewGuid();
            _subscriptionRepository.Setup(repo => repo.GetById(subscriptionId)).ReturnsAsync((Subscription)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionService.DeleteSubscription(subscriptionId));
            _subscriptionRepository.Verify(repo => repo.GetById(subscriptionId), Times.Once);
            _subscriptionRepository.Verify(repo => repo.Delete(subscriptionId), Times.Never);
        }
    }
}
