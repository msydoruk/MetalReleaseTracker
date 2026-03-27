using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetAlbumsMissingCovers;

public class GetAlbumsMissingCoversHandler
{
    private const int DefaultPageSize = 20;

    private readonly CoreDataServiceDbContext _context;

    public GetAlbumsMissingCoversHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<DataQualityPagedResult<DataQualityAlbumDto>> HandleAsync(
        int page = 1,
        int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Albums
            .AsNoTracking()
            .Where(album => album.PhotoUrl == null || album.PhotoUrl == string.Empty)
            .OrderByDescending(album => album.CreatedDate);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(album => new DataQualityAlbumDto
            {
                Id = album.Id,
                Name = album.Name,
                SKU = album.SKU,
                BandName = album.Band.Name,
                DistributorName = album.Distributor.Name,
                PhotoUrl = album.PhotoUrl,
                Status = album.Status.ToString(),
                CreatedDate = album.CreatedDate,
            })
            .ToListAsync(cancellationToken);

        return new DataQualityPagedResult<DataQualityAlbumDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }
}
