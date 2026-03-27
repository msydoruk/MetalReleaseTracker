using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetTopWatchedAlbums;

public class GetTopWatchedAlbumsHandler
{
    private const int TopCount = 20;

    private readonly CoreDataServiceDbContext _context;

    public GetTopWatchedAlbumsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<WatchedAlbumDto>> HandleAsync(
        AnalyticsDateRangeFilter filter,
        CancellationToken cancellationToken = default)
    {
        var watchedAlbums = await _context.UserAlbumWatches
            .AsNoTracking()
            .Include(watch => watch.Band)
            .GroupBy(watch => new { watch.BandId, watch.Band.Name, watch.CanonicalTitle })
            .Select(group => new WatchedAlbumDto
            {
                BandName = group.Key.Name,
                CanonicalTitle = group.Key.CanonicalTitle,
                WatchCount = group.Count(),
            })
            .OrderByDescending(album => album.WatchCount)
            .Take(TopCount)
            .ToListAsync(cancellationToken);

        return watchedAlbums;
    }
}
