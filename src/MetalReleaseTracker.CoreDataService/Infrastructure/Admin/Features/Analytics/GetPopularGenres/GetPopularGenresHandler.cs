using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetPopularGenres;

public class GetPopularGenresHandler
{
    private const int TopCount = 15;
    private const int DefaultWeeks = 12;

    private readonly CoreDataServiceDbContext _context;

    public GetPopularGenresHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<GenreCountDto>> HandleAsync(
        AnalyticsDateRangeFilter filter,
        CancellationToken cancellationToken = default)
    {
        var from = filter.From ?? DateTime.UtcNow.AddDays(-DefaultWeeks * 7);
        var to = filter.To ?? DateTime.UtcNow;

        var genres = await _context.Albums
            .AsNoTracking()
            .Where(album => album.Genre != null && album.Genre != string.Empty)
            .Where(album => album.CreatedDate >= from && album.CreatedDate <= to)
            .GroupBy(album => album.Genre!)
            .Select(group => new GenreCountDto
            {
                Genre = group.Key,
                Count = group.Count(),
            })
            .OrderByDescending(genre => genre.Count)
            .Take(TopCount)
            .ToListAsync(cancellationToken);

        return genres;
    }
}
