using MetalReleaseTracker.CoreDataService.Data.Entities;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IDiscordNotificationService
{
    Task<int> SendNotificationsAsync(List<UserNotificationEntity> notifications, CancellationToken cancellationToken = default);
}
