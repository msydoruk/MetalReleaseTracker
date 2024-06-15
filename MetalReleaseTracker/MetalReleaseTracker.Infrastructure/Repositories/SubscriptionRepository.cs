using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.Infrastructure.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly MetalReleaseTrackerDbContext _dbContext;

        public SubscriptionRepository(MetalReleaseTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(Subscription subscription)
        {
            try
            {
                await _dbContext.Subscriptions.AddAsync(subscription);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error adding subscription: {ex.Message}");
                throw new Exception("An error occurred while adding the subscription.", ex);
            }
        }

        public async Task Delete(Guid id)
        {
            try
            {
                var entity = await _dbContext.Subscriptions.FindAsync(id);
                if (entity == null)
                {
                    throw new KeyNotFoundException($"Subscription with Id '{id}' not found.");
                }

                _dbContext.Subscriptions.Remove(entity);
                await _dbContext.SaveChangesAsync();
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine($"Error deleting subscription: {ex.Message}");
                throw;
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error deleting subscription: {ex.Message}");
                throw new Exception("An error occurred while deleting the subscription.", ex);
            }
        }

        public async Task<IEnumerable<Subscription>> GetAll()
        {
            try
            {
                return await _dbContext.Subscriptions.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving all subscriptions: {ex.Message}");
                throw new Exception("An error occurred while retrieving subscriptions.", ex);
            }
        }

        public async Task<IEnumerable<Subscription>> GetByEmail(string email)
        {
            try
            {
                return await _dbContext.Subscriptions
                    .Where(s => s.Email == email)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving subscriptions by email '{email}': {ex.Message}");
                throw new Exception("An error occurred while retrieving subscriptions by email.", ex);
            }
        }

        public async Task<Subscription> GetById(Guid id)
        {
            try
            {
                var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == id);
                if (subscription == null)
                {
                    throw new KeyNotFoundException($"Subscription with Id '{id}' not found.");
                }

                return subscription;
            }
            catch (KeyNotFoundException ex)
            {
                Console.Error.WriteLine($"Error retrieving subscription by id '{id}': {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving subscription by id '{id}': {ex.Message}");
                throw new Exception("An error occurred while retrieving the subscription by id.", ex);
            }
        }

        public async Task<IEnumerable<Subscription>> GetByNotifyForNewReleases(bool notify)
        {
            try
            {
                return await _dbContext.Subscriptions
                    .Where(s => s.NotifyForNewReleases == notify)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving subscriptions by notify flag '{notify}': {ex.Message}");
                throw new Exception("An error occurred while retrieving subscriptions by notify flag.", ex);
            }
        }

        public async Task Update(Subscription subscription)
        {
            try
            {
                _dbContext.Subscriptions.Update(subscription);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.Error.WriteLine($"Error updating subscription: {ex.Message}");
                throw new Exception("An error occurred while updating the subscription.", ex);
            }
        }
    }
}
