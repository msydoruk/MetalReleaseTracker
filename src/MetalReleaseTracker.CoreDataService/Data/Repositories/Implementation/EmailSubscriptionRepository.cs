using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class EmailSubscriptionRepository : IEmailSubscriptionRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public EmailSubscriptionRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EmailSubscriptionEntity?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailSubscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(subscription => subscription.UserId == userId, cancellationToken);
    }

    public async Task<EmailSubscriptionEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailSubscriptions
            .FirstOrDefaultAsync(subscription => subscription.VerificationToken == token, cancellationToken);
    }

    public async Task<Dictionary<string, string>> GetEmailsByUserIdsAsync(List<string> userIds, CancellationToken cancellationToken = default)
    {
        return await _dbContext.EmailSubscriptions
            .AsNoTracking()
            .Where(subscription => userIds.Contains(subscription.UserId)
                && subscription.IsVerified
                && subscription.UnsubscribedAt == null)
            .ToDictionaryAsync(subscription => subscription.UserId, subscription => subscription.Email, cancellationToken);
    }

    public async Task AddAsync(EmailSubscriptionEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.EmailSubscriptions.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(EmailSubscriptionEntity entity, CancellationToken cancellationToken = default)
    {
        _dbContext.EmailSubscriptions.Update(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _dbContext.EmailSubscriptions
            .Where(subscription => subscription.UserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
