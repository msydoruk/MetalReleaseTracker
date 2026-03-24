using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Extensions;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class UserNotificationRepository : IUserNotificationRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public UserNotificationRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddBatchAsync(List<UserNotificationEntity> entities, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserNotifications.AddRangeAsync(entities, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResultDto<UserNotificationEntity>> GetPagedAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.UserNotifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .Include(notification => notification.Album)
                .ThenInclude(album => album.Band)
            .OrderByDescending(notification => notification.CreatedDate);

        return await query.ToPagedResultAsync(page, pageSize, cancellationToken);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserNotifications
            .AsNoTracking()
            .CountAsync(notification => notification.UserId == userId && !notification.IsRead, cancellationToken);
    }

    public async Task MarkAsReadAsync(string userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.UserNotifications
            .FirstOrDefaultAsync(
                notification => notification.UserId == userId && notification.Id == notificationId,
                cancellationToken);

        if (entity != null)
        {
            entity.IsRead = true;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _dbContext.UserNotifications
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ExecuteUpdateAsync(
                notifications => notifications.SetProperty(notification => notification.IsRead, true),
                cancellationToken);
    }
}
