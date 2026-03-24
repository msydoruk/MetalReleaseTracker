using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IUserNotificationRepository
{
    Task AddBatchAsync(List<UserNotificationEntity> entities, CancellationToken cancellationToken = default);

    Task<PagedResultDto<UserNotificationEntity>> GetPagedAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(string userId, Guid notificationId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
}
