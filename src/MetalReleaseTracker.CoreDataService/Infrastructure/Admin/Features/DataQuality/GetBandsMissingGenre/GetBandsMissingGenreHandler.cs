using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetBandsMissingGenre;

public class GetBandsMissingGenreHandler
{
    private const int DefaultPageSize = 20;

    private readonly CoreDataServiceDbContext _context;

    public GetBandsMissingGenreHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<DataQualityPagedResult<DataQualityBandDto>> HandleAsync(
        int page = 1,
        int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Bands
            .AsNoTracking()
            .Where(band => band.Genre == null || band.Genre == string.Empty)
            .OrderBy(band => band.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(band => new DataQualityBandDto
            {
                Id = band.Id,
                Name = band.Name,
                Genre = band.Genre,
                PhotoUrl = band.PhotoUrl,
                IsVisible = band.IsVisible,
                AlbumCount = _context.Albums.Count(album => album.BandId == band.Id),
            })
            .ToListAsync(cancellationToken);

        return new DataQualityPagedResult<DataQualityBandDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
