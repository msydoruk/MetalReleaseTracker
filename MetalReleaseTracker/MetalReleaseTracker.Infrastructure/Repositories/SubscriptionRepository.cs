using AutoMapper;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using MetalReleaseTracker.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        public async Task Add(Subscription subscription)
        {
            var subscriptionEntity = _mapper.Map<SubscriptionEntity>(subscription);
            await _dbContext.Subscriptions.AddAsync(subscriptionEntity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Delete(Guid id)
        {
            var subscription = await _dbContext.Subscriptions.FindAsync(id);
            _dbContext.Subscriptions.Remove(subscription);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Subscription>> GetAll()
        {
            var subscriptions = await _dbContext.Subscriptions
                    .AsNoTracking()
                    .ToListAsync();
            return _mapper.Map<IEnumerable<Subscription>>(subscriptions);
        }

        public async Task<Subscription> GetById(Guid id)
        {
            var subscription = await _dbContext.Subscriptions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);
            return _mapper.Map<Subscription>(subscription);
        }

        public async Task Update(Subscription subscription)
        {
            var subscriptionEntity = _mapper.Map<SubscriptionEntity>(subscription);
            _dbContext.Subscriptions.Update(subscriptionEntity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
