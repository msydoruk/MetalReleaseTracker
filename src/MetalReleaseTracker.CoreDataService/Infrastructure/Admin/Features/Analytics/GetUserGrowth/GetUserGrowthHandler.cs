using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetUserGrowth;

public class GetUserGrowthHandler
{
    private const int DefaultWeeks = 12;

    private readonly CoreDataServiceDbContext _context;

    public GetUserGrowthHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<TimeSeriesDataPoint>> HandleAsync(
        AnalyticsDateRangeFilter filter,
        CancellationToken cancellationToken = default)
    {
        var from = filter.From ?? DateTime.UtcNow.AddDays(-DefaultWeeks * 7);
        var to = filter.To ?? DateTime.UtcNow;

        var points = await _context.Database
            .SqlQueryRaw<TimeSeriesDataPoint>(
                """
                SELECT DATE_TRUNC('week', "CreatedDate")::timestamp AS "Date",
                       COUNT(*)::int AS "Count"
                FROM "AspNetUsers"
                WHERE "CreatedDate" >= {0} AND "CreatedDate" <= {1}
                GROUP BY DATE_TRUNC('week', "CreatedDate")
                ORDER BY "Date"
                """,
                from,
                to)
            .ToListAsync(cancellationToken);

        return points;
    }
}
