using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;

public class GetBandsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetBandsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminBandPagedResult> HandleAsync(
        AdminBandFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Bands.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(band => EF.Functions.ILike(band.Name, $"%{filter.Search}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(band => band.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(band => new AdminBandDto
            {
                Id = band.Id,
                Name = band.Name,
                Genre = band.Genre,
                PhotoUrl = band.PhotoUrl,
                MetalArchivesUrl = band.MetalArchivesUrl,
                FormationYear = band.FormationYear,
                AlbumCount = _context.Albums.Count(album => album.BandId == band.Id),
                Slug = band.Slug,
            })
            .ToListAsync(cancellationToken);

        return new AdminBandPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }
}
