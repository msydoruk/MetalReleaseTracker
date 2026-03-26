using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;

public class GetAlbumsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetAlbumsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminAlbumPagedResult> HandleAsync(
        AdminAlbumFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Albums.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(album =>
                EF.Functions.ILike(album.Name, $"%{filter.Search}%") ||
                EF.Functions.ILike(album.SKU, $"%{filter.Search}%"));
        }

        if (filter.BandId.HasValue)
        {
            query = query.Where(album => album.BandId == filter.BandId.Value);
        }

        if (filter.DistributorId.HasValue)
        {
            query = query.Where(album => album.DistributorId == filter.DistributorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<AlbumStatus>(filter.Status, ignoreCase: true, out var albumStatus))
        {
            query = query.Where(album => album.Status == albumStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var albums = await query
            .OrderByDescending(album => album.CreatedDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .Include(album => album.Translations)
            .ToListAsync(cancellationToken);

        var items = albums.Select(album => new AdminAlbumDto
        {
            Id = album.Id,
            Name = album.Name,
            BandName = album.Band.Name,
            DistributorName = album.Distributor.Name,
            SKU = album.SKU,
            Price = album.Price,
            Status = album.Status?.ToString(),
            StockStatus = album.StockStatus?.ToString(),
            Media = album.Media?.ToString(),
            CreatedDate = album.CreatedDate,
            Slug = album.Slug,
            Translations = album.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new AlbumTranslationDto
                {
                    SeoTitle = translation.SeoTitle,
                    SeoDescription = translation.SeoDescription,
                    SeoKeywords = translation.SeoKeywords,
                }),
        }).ToList();

        return new AdminAlbumPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }
}
