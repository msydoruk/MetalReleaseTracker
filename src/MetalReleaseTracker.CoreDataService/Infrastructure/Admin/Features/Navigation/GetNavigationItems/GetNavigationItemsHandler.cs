using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;

public class GetNavigationItemsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetNavigationItemsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<NavigationItemDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var items = await _context.NavigationItems
            .AsNoTracking()
            .Include(item => item.Translations)
            .OrderBy(item => item.SortOrder)
            .ToListAsync(cancellationToken);

        return items.Select(item => new NavigationItemDto
        {
            Id = item.Id,
            Path = item.Path,
            IconName = item.IconName,
            SortOrder = item.SortOrder,
            IsVisible = item.IsVisible,
            IsProtected = item.IsProtected,
            CreatedDate = item.CreatedDate,
            UpdatedAt = item.UpdatedAt,
            Translations = item.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new NavigationItemTranslationDto
                {
                    Title = translation.Title,
                    SeoTitle = translation.SeoTitle,
                    SeoDescription = translation.SeoDescription,
                    SeoKeywords = translation.SeoKeywords,
                }),
        }).ToList();
    }
}
