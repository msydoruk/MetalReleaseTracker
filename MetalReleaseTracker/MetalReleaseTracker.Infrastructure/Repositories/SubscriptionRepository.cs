using AutoMapper;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class SubscriptionRepository : BaseRepository<SubscriptionRepository>, ISubscriptionRepository
    {
        public SubscriptionRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper, ILogger<SubscriptionRepository> logger)
           : base(dbContext, mapper, logger) { }

        public async Task Add(Subscription subscription)
        {
            await HandleDbUpdateException(async () =>
            {
                var subscriptionEntity = Mapper.Map<SubscriptionEntity>(subscription);
                await DbContext.Subscriptions.AddAsync(subscriptionEntity);
                await DbContext.SaveChangesAsync();
            });
        }

        public async Task Delete(Guid id)
        {
            await HandleDbUpdateException(async () =>
            {
                var subscription = await DbContext.Subscriptions.FindAsync(id);
                if (subscription == null)
                {
                    Logger.LogWarning("Subscription with Id {Id} not found.", id);
                    throw new KeyNotFoundException($"Subscription with Id '{id}' not found.");
                }

                DbContext.Subscriptions.Remove(subscription);
                await DbContext.SaveChangesAsync();
            });
        }

        public async Task<IEnumerable<Subscription>> GetAll()
        {
            return await HandleDbUpdateException(async () =>
            {
                var subscriptions = await DbContext.Subscriptions
                    .AsNoTracking()
                    .ToListAsync();

                return Mapper.Map<IEnumerable<Subscription>>(subscriptions);
            });
        }

        public async Task<Subscription> GetById(Guid id)
        {
            return await HandleDbUpdateException(async () =>
            {
                var subscription = await DbContext.Subscriptions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subscription == null)
                {
                    Logger.LogWarning("Subscription with Id {Id} not found.", id);
                }

                return Mapper.Map<Subscription>(subscription);
            });
        }

        public async Task Update(Subscription subscription)
        {
            await HandleDbUpdateException(async () =>
            {
                var subscriptionEntity = Mapper.Map<SubscriptionEntity>(subscription);
                DbContext.Subscriptions.Update(subscriptionEntity);
                await DbContext.SaveChangesAsync();
            });
        }
    }
}
