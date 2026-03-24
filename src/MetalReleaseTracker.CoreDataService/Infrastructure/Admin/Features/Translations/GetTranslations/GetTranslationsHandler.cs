using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.GetTranslations;

public class GetTranslationsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetTranslationsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<TranslationPagedResult> HandleAsync(
        TranslationFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Translations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Category))
        {
            query = query.Where(translation => translation.Category == filter.Category);
        }

        if (!string.IsNullOrWhiteSpace(filter.Language))
        {
            query = query.Where(translation => translation.Language == filter.Language);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(translation =>
                EF.Functions.ILike(translation.Key, $"%{filter.Search}%") ||
                EF.Functions.ILike(translation.Value, $"%{filter.Search}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(translation => translation.Category)
            .ThenBy(translation => translation.Key)
            .ThenBy(translation => translation.Language)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(translation => new TranslationDto
            {
                Id = translation.Id,
                Key = translation.Key,
                Language = translation.Language,
                Value = translation.Value,
                Category = translation.Category,
                UpdatedAt = translation.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return new TranslationPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }
}
