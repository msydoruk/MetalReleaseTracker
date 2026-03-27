using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetPotentialDuplicateBands;

public class GetPotentialDuplicateBandsHandler
{
    private const int DefaultPageSize = 20;

    private readonly CoreDataServiceDbContext _context;

    public GetPotentialDuplicateBandsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<DataQualityPagedResult<DuplicateBandGroupDto>> HandleAsync(
        int page = 1,
        int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var duplicateNames = await _context.Bands
            .AsNoTracking()
            .GroupBy(band => band.Name.ToLower().Trim())
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToListAsync(cancellationToken);

        var totalCount = duplicateNames.Count;

        var pagedNames = duplicateNames
            .OrderBy(name => name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var bands = await _context.Bands
            .AsNoTracking()
            .Where(band => pagedNames.Contains(band.Name.ToLower().Trim()))
            .Select(band => new
            {
                band.Id,
                band.Name,
                band.Genre,
                band.IsVisible,
                NormalizedName = band.Name.ToLower().Trim(),
                AlbumCount = _context.Albums.Count(album => album.BandId == band.Id),
            })
            .OrderBy(band => band.NormalizedName)
            .ThenBy(band => band.Name)
            .ToListAsync(cancellationToken);

        var groups = bands
            .GroupBy(band => band.NormalizedName)
            .Select(group => new DuplicateBandGroupDto
            {
                NormalizedName = group.Key,
                Bands = group.Select(band => new DuplicateBandDto
                {
                    Id = band.Id,
                    Name = band.Name,
                    Genre = band.Genre,
                    AlbumCount = band.AlbumCount,
                    IsVisible = band.IsVisible,
                }).ToList(),
            })
            .ToList();

        return new DataQualityPagedResult<DuplicateBandGroupDto>
        {
            Items = groups,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
