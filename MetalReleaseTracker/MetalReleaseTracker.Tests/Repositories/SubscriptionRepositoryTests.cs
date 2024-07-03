using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Repositories;
using MetalReleaseTracker.Tests.Base;

namespace MetalReleaseTracker.Tests.Repositories
{
    public class SubscriptionRepositoryTests : IntegrationTestBase
    {
        private readonly SubscriptionRepository _repository;

        public SubscriptionRepositoryTests()
        {
            _repository = new SubscriptionRepository(DbContext, Mapper);
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
        public async Task Add_ShouldAddSubscription()
        {
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Email = "new@example.com",
                NotifyForNewReleases = true
            };

            await _repository.Add(subscription);

            var result = await DbContext.Subscriptions.FindAsync(subscription.Id);

            Assert.NotNull(result);
            Assert.Equal(subscription.Email, result.Email);
            Assert.Equal(subscription.NotifyForNewReleases, result.NotifyForNewReleases);
        }

        [Fact]
        public async Task Add_ShouldNotAddSubscription_WhenEmailIsInvalid()
        {
            var subscription = new Subscription
            {
                Email = "",
                NotifyForNewReleases = true
            };

            await _repository.Add(subscription);

            var result = await DbContext.Subscriptions.FindAsync(subscription.Id);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllSubscriptions()
        {
            var result = await _repository.GetAll();

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetById_ShouldReturnSubscription_WhenIdExists()
        {
            var subscriptionEntity = DbContext.Subscriptions.First();
            var result = await _repository.GetById(subscriptionEntity.Id);

            Assert.NotNull(result);
            Assert.Equal(subscriptionEntity.Id, result.Id);
            Assert.Equal(subscriptionEntity.Email, result.Email);
            Assert.Equal(subscriptionEntity.NotifyForNewReleases, result.NotifyForNewReleases);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenIdDoesNotExist()
        {
            var result = await _repository.GetById(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task Update_ShouldUpdateSubscription()
        {
            var subscriptionEntity = DbContext.Subscriptions.First();
            var subscription = Mapper.Map<Subscription>(subscriptionEntity);

            subscription.Email = "updated@example.com";
            subscription.NotifyForNewReleases = !subscription.NotifyForNewReleases;

            DbContext.Entry(subscriptionEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var result = await _repository.Update(subscription);

            var updatedEntity = await DbContext.Subscriptions.FindAsync(subscription.Id);

            Assert.True(result);
            Assert.NotNull(updatedEntity);
            Assert.Equal("updated@example.com", updatedEntity.Email);
            Assert.Equal(subscription.NotifyForNewReleases, updatedEntity.NotifyForNewReleases);
        }

        [Fact]
        public async Task Update_ShouldReturnFalse_WhenSubscriptionDoesNotExist()
        {
            var subscription = new Subscription
            {
                Id = Guid.NewGuid(),
                Email = "nonexistent@example.com",
                NotifyForNewReleases = true
            };

            var result = await _repository.Update(subscription);

            Assert.False(result);
        }

        [Fact]
        public async Task Delete_ShouldRemoveSubscription()
        {
            var subscriptionEntity = DbContext.Subscriptions.First();
            var result = await _repository.Delete(subscriptionEntity.Id);

            var deletedEntity = await DbContext.Subscriptions.FindAsync(subscriptionEntity.Id);

            Assert.True(result);
            Assert.Null(deletedEntity);
        }

        [Fact]
        public async Task Delete_ShouldReturnFalse_WhenSubscriptionDoesNotExist()
        {
            var result = await _repository.Delete(Guid.NewGuid());

            Assert.False(result);
        }
    }
}
