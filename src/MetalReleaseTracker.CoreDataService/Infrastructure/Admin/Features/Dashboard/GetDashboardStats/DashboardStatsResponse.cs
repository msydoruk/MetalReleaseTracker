namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Dashboard.GetDashboardStats;

public class DashboardStatsResponse
{
    public int TotalAlbums { get; set; }

    public int TotalBands { get; set; }

    public int TotalDistributors { get; set; }

    public int TotalUsers { get; set; }

    public int NewAlbumsThisMonth { get; set; }

    public int PreOrders { get; set; }

    public int TotalTranslations { get; set; }

    public int PublishedNews { get; set; }
}
