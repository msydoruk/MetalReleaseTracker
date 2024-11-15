using AutoMapper;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;
using MetalReleaseTracker.Infrastructure.Repositories;

namespace MetalReleaseTracker.Tests.Repositories
{
    public class SubscriptionRepositoryTests : IAsyncLifetime
    {
        private MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly SubscriptionRepository _repository;

        public SubscriptionRepositoryTests()
        {
            var configuration = new MapperConfiguration(configuration =>
            {
                configuration.AddProfile<MappingProfile>();
            });

            _dbContext = TestDbContextFactory.CreateDbContext();
            _mapper = configuration.CreateMapper();
            _repository = new SubscriptionRepository(_dbContext, _mapper);
        }

        public async Task InitializeAsync()
        {
            await InitializeData(_dbContext);
        }

        public Task DisposeAsync()
        {
            TestDbContextFactory.ClearDatabase(_dbContext);

            return Task.CompletedTask;
        }

        protected async Task InitializeData(MetalReleaseTrackerDbContext context)
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

            var result = await _dbContext.Subscriptions.FindAsync(subscription.Id);

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

            var result = await _dbContext.Subscriptions.FindAsync(subscription.Id);

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
            var subscriptionEntity = _dbContext.Subscriptions.First();
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
            var subscriptionEntity = _dbContext.Subscriptions.First();
            var subscription = _mapper.Map<Subscription>(subscriptionEntity);

            subscription.Email = "updated@example.com";
            subscription.NotifyForNewReleases = !subscription.NotifyForNewReleases;

            _dbContext.Entry(subscriptionEntity).State = Microsoft.EntityFrameworkCore.EntityState.Detached;

            var result = await _repository.Update(subscription);

            var updatedEntity = await _dbContext.Subscriptions.FindAsync(subscription.Id);

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
            var subscriptionEntity = _dbContext.Subscriptions.First();
            var result = await _repository.Delete(subscriptionEntity.Id);

            var deletedEntity = await _dbContext.Subscriptions.FindAsync(subscriptionEntity.Id);

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
