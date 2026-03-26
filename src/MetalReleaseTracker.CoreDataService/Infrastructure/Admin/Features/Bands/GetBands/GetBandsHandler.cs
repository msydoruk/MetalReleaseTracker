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

        var bands = await query
            .OrderBy(band => band.Name)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Include(band => band.Translations)
            .ToListAsync(cancellationToken);

        var bandIds = bands.Select(band => band.Id).ToList();
        var albumCounts = await _context.Albums
            .Where(album => bandIds.Contains(album.BandId))
            .GroupBy(album => album.BandId)
            .Select(group => new { BandId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(
                group => group.BandId,
                group => group.Count,
                cancellationToken);

        var items = bands.Select(band => new AdminBandDto
        {
            Id = band.Id,
            Name = band.Name,
            Genre = band.Genre,
            PhotoUrl = band.PhotoUrl,
            MetalArchivesUrl = band.MetalArchivesUrl,
            FormationYear = band.FormationYear,
            AlbumCount = albumCounts.GetValueOrDefault(band.Id),
            Slug = band.Slug,
            IsVisible = band.IsVisible,
            Translations = band.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new BandTranslationDto
                {
                    Description = translation.Description,
                    SeoTitle = translation.SeoTitle,
                    SeoDescription = translation.SeoDescription,
                    SeoKeywords = translation.SeoKeywords,
                }),
        }).ToList();

        return new AdminBandPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }
}
