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
    public class SubscriptionServiceTest : IntegrationTestBase
    {
        private readonly SubscriptionRepository _subscriptionRepository;
        private readonly ValidationService _validationService;
        private readonly SubscriptionService _subscriptionService;

        public SubscriptionServiceTest()
        {
            _subscriptionRepository = new SubscriptionRepository(DbContext, Mapper);
            _validationService = new ValidationService(new List<IValidator>
            {
                new SubscriptionValidator(),
                new GuidValidator()
            });
            _subscriptionService = new SubscriptionService(_subscriptionRepository, _validationService);
        }

        protected override void InitializeData(MetalReleaseTrackerDbContext context)
        {
            var subscriptions = new[]
            {
                new SubscriptionEntity 
                { 
                    Id = Guid.NewGuid(), 
                    Email = "test1@example.com",
                    NotifyForNewReleases = true
                },
                new SubscriptionEntity 
                { 
                    Id = Guid.NewGuid(), 
                    Email = "test2@example.com",  
                    NotifyForNewReleases = false
                }
            };
            context.Subscriptions.AddRange(subscriptions);
            context.SaveChanges();
        }

        [Fact]
        public async Task GetSubscriptionById_ShouldReturnSubscription_WhenIdExists()
        {
            var existingSubscription = await DbContext.Subscriptions.FirstAsync();
            var result = await _subscriptionService.GetSubscriptionById(existingSubscription.Id);

            Assert.NotNull(result);
            Assert.Equal(existingSubscription.Email, result.Email);
        }

        [Fact]
        public async Task GetSubscriptionById_ShouldThrowEntityNotFoundException_WhenIdDoesNotExist()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionService.GetSubscriptionById(Guid.NewGuid()));
        }

        [Fact]
        public async Task GetAllSubscriptions_ShouldReturnAllSubscriptions()
        {
            var result = await _subscriptionService.GetAllSubscriptions();

            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count());
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
            await _subscriptionService.AddSubscription(newSubscription);

            var result = await DbContext.Subscriptions.FindAsync(newSubscription.Id);

            Assert.NotNull(result);
            Assert.Equal(newSubscription.Email, result.Email);
        }

        [Fact]
        public async Task AddSubscription_ShouldThrowValidationException_WhenSubscriptionIsInvalid()
        {
            var subscription = new Subscription
            {
                Email =  ""
            };

            await Assert.ThrowsAsync<ValidationException>(() => _subscriptionService.AddSubscription(subscription));
        }

        [Fact]
        public async Task UpdateSubscription_ShouldUpdateSubscription()
        {
            var existingSubscription = await DbContext.Subscriptions.FirstAsync();
            var updatedSubscription = Mapper.Map<Subscription>(existingSubscription);
            updatedSubscription.Email = "updated@example.com";
            var result = await _subscriptionService.UpdateSubscription(updatedSubscription);

            var retrievedSubscription = await DbContext.Subscriptions.FindAsync(existingSubscription.Id);

            Assert.True(result);
            Assert.NotNull(retrievedSubscription);
            Assert.Equal(updatedSubscription.Email, retrievedSubscription.Email);
        }

        [Fact]
        public async Task UpdateSubscription_ShouldThrowEntityNotFoundException_WhenSubscriptionDoesNotExist()
        {
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Email = "invalid@gmail.com"
            };

            await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionService.UpdateSubscription(subscription));
        }

        [Fact]
        public async Task DeleteSubscription_ShouldRemoveSubscription()
        {
            var subscription = await DbContext.Subscriptions.FirstAsync();
            var result = await _subscriptionService.DeleteSubscription(subscription.Id);

            var deletedSubscription = await DbContext.Subscriptions.FindAsync(subscription.Id);

            Assert.True(result);
            Assert.Null(deletedSubscription);
        }

        [Fact]
        public async Task DeleteSubscription_ShouldThrowEntityNotFoundException_WhenSubscriptionDoesNotExist()
        {
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _subscriptionService.DeleteSubscription(Guid.NewGuid()));
        }
    }
}
