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
        private readonly ILogger<SubscriptionRepository> _logger;

        public SubscriptionRepository(MetalReleaseTrackerDbContext dbContext, IMapper mapper, ILogger<SubscriptionRepository> logger)
        {
            _dbContext = dbContext;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task Add(Subscription subscription)
        {
            try
            {
                var subscriptionEntity = _mapper.Map<SubscriptionEntity>(subscription);
                await _dbContext.Subscriptions.AddAsync(subscriptionEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while adding a subscription.");
                throw;
            }
        }

        public async Task Delete(Guid id)
        {
            try
            {
                var subscription = await _dbContext.Subscriptions.FindAsync(id);
                if (subscription != null)
                {
                    _dbContext.Subscriptions.Remove(subscription);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    _logger.LogWarning($"Attempted to delete subscription with ID {id}, but no matching subscription was found.");
                }
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while deleting a subscription.");
                throw;
            }
        }

        public async Task<IEnumerable<Subscription>> GetAll()
        {
            try
            {
                var subscriptions = await _dbContext.Subscriptions
                    .AsNoTracking()
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Subscription>>(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all subscriptions.");
                throw;
            }
        }

        public async Task<IEnumerable<Subscription>> GetByEmail(string email)
        {
            try
            {
                var subscriptions = await _dbContext.Subscriptions
                    .AsNoTracking()
                    .Where(s => s.Email == email)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Subscription>>(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving subscriptions by email.");
                throw;
            }
        }

        public async Task<Subscription> GetById(Guid id)
        {
            try
            {
                var subscription = await _dbContext.Subscriptions
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == id);

                return _mapper.Map<Subscription>(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while retrieving a subscription with ID {id}.");
                throw;
            }
        }

        public async Task<IEnumerable<Subscription>> GetByNotifyForNewReleases(bool notify)
        {
            try
            {
                var subscriptions = await _dbContext.Subscriptions
                    .AsNoTracking()
                    .Where(s => s.NotifyForNewReleases == notify)
                    .ToListAsync();

                return _mapper.Map<IEnumerable<Subscription>>(subscriptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving subscriptions by notify flag.");
                throw;
            }
        }

        public async Task Update(Subscription subscription)
        {
            try
            {
                var subscriptionEntity = _mapper.Map<SubscriptionEntity>(subscription);
                _dbContext.Subscriptions.Update(subscriptionEntity);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "An error occurred while updating a subscription.");
                throw;
            }
        }
    }
}
