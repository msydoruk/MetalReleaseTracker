using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Events;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface INotificationService
{
    Task GenerateNotificationsAsync(AlbumProcessedPublicationEvent albumEvent, AlbumEntity? existingAlbum, Guid bandId, CancellationToken cancellationToken = default);

    Task<PagedResultDto<NotificationDto>> GetNotificationsAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(string userId, Guid notificationId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
}
