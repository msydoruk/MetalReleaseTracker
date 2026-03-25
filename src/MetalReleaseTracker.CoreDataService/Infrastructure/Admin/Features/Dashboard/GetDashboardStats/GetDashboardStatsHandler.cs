using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Dashboard.GetDashboardStats;

public class GetDashboardStatsHandler
{
    private readonly CoreDataServiceDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public GetDashboardStatsHandler(
        CoreDataServiceDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<DashboardStatsResponse> HandleAsync(CancellationToken cancellationToken = default)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var totalAlbums = await _context.Albums.CountAsync(cancellationToken);
        var totalBands = await _context.Bands.CountAsync(cancellationToken);
        var totalDistributors = await _context.Distributors.CountAsync(cancellationToken);
        var totalUsers = await _userManager.Users.CountAsync(cancellationToken);
        var newAlbumsThisMonth = await _context.Albums
            .CountAsync(
                album => album.CreatedDate >= thirtyDaysAgo,
                cancellationToken);
        var preOrders = await _context.Albums
            .CountAsync(
                album => album.Status == Data.Entities.Enums.AlbumStatus.Preorder,
                cancellationToken);
        var totalTranslations = await _context.Translations.CountAsync(cancellationToken);
        var publishedNews = await _context.NewsArticles
            .CountAsync(
                news => news.IsPublished,
                cancellationToken);

        return new DashboardStatsResponse
        {
            TotalAlbums = totalAlbums,
            TotalBands = totalBands,
            TotalDistributors = totalDistributors,
            TotalUsers = totalUsers,
            NewAlbumsThisMonth = newAlbumsThisMonth,
            PreOrders = preOrders,
            TotalTranslations = totalTranslations,
            PublishedNews = publishedNews,
        };
    }
}
