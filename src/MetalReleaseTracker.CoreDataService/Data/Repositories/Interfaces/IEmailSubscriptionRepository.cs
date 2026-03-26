using MetalReleaseTracker.CoreDataService.Data.Entities;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IEmailSubscriptionRepository
{
    Task<EmailSubscriptionEntity?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    Task<EmailSubscriptionEntity?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<Dictionary<string, string>> GetEmailsByUserIdsAsync(List<string> userIds, CancellationToken cancellationToken = default);

    Task AddAsync(EmailSubscriptionEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(EmailSubscriptionEntity entity, CancellationToken cancellationToken = default);

    Task RemoveByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}
