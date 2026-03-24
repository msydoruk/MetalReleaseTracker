using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Notifications.GetNotificationStats;

public class GetNotificationStatsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetNotificationStatsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationStatsResponse> HandleAsync(CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.UserNotifications
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var unreadCount = await _context.UserNotifications
            .AsNoTracking()
            .CountAsync(
                notification => !notification.IsRead,
                cancellationToken);

        var byType = await _context.UserNotifications
            .AsNoTracking()
            .GroupBy(notification => notification.NotificationType)
            .Select(group => new
            {
                Type = group.Key.ToString(),
                Count = group.Count(),
            })
            .ToDictionaryAsync(
                item => item.Type,
                item => item.Count,
                cancellationToken);

        return new NotificationStatsResponse
        {
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            ByType = byType,
        };
    }
}
