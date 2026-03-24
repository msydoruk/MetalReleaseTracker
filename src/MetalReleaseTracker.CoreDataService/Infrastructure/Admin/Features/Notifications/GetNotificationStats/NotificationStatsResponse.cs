namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Notifications.GetNotificationStats;

public class NotificationStatsResponse
{
    public int TotalCount { get; set; }

    public int UnreadCount { get; set; }

    public Dictionary<string, int> ByType { get; set; } = new();
}
