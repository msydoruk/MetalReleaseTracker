using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IEmailNotificationService
{
    Task<int> SendNotificationsAsync(List<UserNotificationEntity> notifications, CancellationToken cancellationToken = default);

    Task SubscribeAsync(string userId, string email, CancellationToken cancellationToken = default);

    Task<bool> VerifyAsync(string token, CancellationToken cancellationToken = default);

    Task UnsubscribeAsync(string userId, CancellationToken cancellationToken = default);

    Task<EmailSubscriptionStatusDto> GetStatusAsync(string userId, CancellationToken cancellationToken = default);
}
