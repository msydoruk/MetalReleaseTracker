using AutoMapper;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;
        private readonly IMapper _mapper;

        public SubscriptionRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<Subscription> GetById(Guid id)
        {
            var subscription = await _dbContext.Subscriptions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);
            return _mapper.Map<Subscription>(subscription);
        }

        public async Task<IEnumerable<Subscription>> GetAll()
        {
            var subscriptions = await _dbContext.Subscriptions
                    .AsNoTracking()
                    .ToListAsync();
            return _mapper.Map<IEnumerable<Subscription>>(subscriptions);
        }

        public async Task Add(Subscription subscription)
        {
            var subscriptionEntity = _mapper.Map<SubscriptionEntity>(subscription);
            await _dbContext.Subscriptions.AddAsync(subscriptionEntity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> Update(Subscription subscription)
        {
            var existingSubscription = await _dbContext.Subscriptions.FindAsync(subscription.Id);

            if (existingSubscription == null)
            {
                return false;
            }

           _mapper.Map(subscription, existingSubscription);

            var changes = await _dbContext.SaveChangesAsync();

            return changes > 0;
        }

        public async Task<bool> Delete(Guid id)
        {
            var subscription = await _dbContext.Subscriptions.FindAsync(id);

            if (subscription == null)
            {
                return false;
            }

            _dbContext.Subscriptions.Remove(subscription);
            var changes = await _dbContext.SaveChangesAsync();

            return changes > 0;
        }
    }
}
