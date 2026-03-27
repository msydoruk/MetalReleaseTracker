using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetAlbumsPerWeek;

public class GetAlbumsPerWeekHandler
{
    private const int DefaultWeeks = 12;

    private readonly CoreDataServiceDbContext _context;

    public GetAlbumsPerWeekHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<TimeSeriesDataPoint>> HandleAsync(
        AnalyticsDateRangeFilter filter,
        CancellationToken cancellationToken = default)
    {
        var from = filter.From ?? DateTime.UtcNow.AddDays(-DefaultWeeks * 7);
        var to = filter.To ?? DateTime.UtcNow;

        var albums = await _context.Albums
            .AsNoTracking()
            .Where(album => album.CreatedDate >= from && album.CreatedDate <= to)
            .Select(album => album.CreatedDate)
            .ToListAsync(cancellationToken);

        var grouped = albums
            .GroupBy(date => StartOfWeek(date))
            .Select(group => new TimeSeriesDataPoint
            {
                Date = group.Key,
                Count = group.Count(),
            })
            .OrderBy(point => point.Date)
            .ToList();

        return grouped;
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }
}
