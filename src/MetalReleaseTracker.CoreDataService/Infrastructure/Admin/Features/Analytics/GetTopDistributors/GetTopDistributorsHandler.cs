using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetTopDistributors;

public class GetTopDistributorsHandler
{
    private const int DefaultWeeks = 12;

    private readonly CoreDataServiceDbContext _context;

    public GetTopDistributorsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<DistributorAlbumCountDto>> HandleAsync(
        AnalyticsDateRangeFilter filter,
        CancellationToken cancellationToken = default)
    {
        var from = filter.From ?? DateTime.UtcNow.AddDays(-DefaultWeeks * 7);
        var to = filter.To ?? DateTime.UtcNow;

        var distributors = await _context.Albums
            .AsNoTracking()
            .Include(album => album.Distributor)
            .Where(album => album.CreatedDate >= from && album.CreatedDate <= to)
            .GroupBy(album => new { album.DistributorId, album.Distributor.Name })
            .Select(group => new DistributorAlbumCountDto
            {
                DistributorId = group.Key.DistributorId,
                DistributorName = group.Key.Name,
                AlbumCount = group.Count(),
            })
            .OrderByDescending(distributor => distributor.AlbumCount)
            .ToListAsync(cancellationToken);

        return distributors;
    }
}
