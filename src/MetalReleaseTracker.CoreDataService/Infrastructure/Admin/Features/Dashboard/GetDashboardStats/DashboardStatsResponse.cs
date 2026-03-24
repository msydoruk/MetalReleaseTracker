namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Dashboard.GetDashboardStats;

public class DashboardStatsResponse
{
    public int TotalAlbums { get; set; }

    public int TotalBands { get; set; }

    public int TotalDistributors { get; set; }

    public int TotalUsers { get; set; }

    public int RecentAlbums { get; set; }

    public int TotalReviews { get; set; }

    public int TotalNotifications { get; set; }

    public int TelegramLinkedUsers { get; set; }
}
