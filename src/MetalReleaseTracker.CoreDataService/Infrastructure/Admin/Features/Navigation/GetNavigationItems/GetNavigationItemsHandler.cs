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
            .OrderBy(item => item.SortOrder)
            .Select(item => new NavigationItemDto
            {
                Id = item.Id,
                TitleEn = item.TitleEn,
                TitleUa = item.TitleUa,
                Path = item.Path,
                IconName = item.IconName,
                SortOrder = item.SortOrder,
                IsVisible = item.IsVisible,
                IsProtected = item.IsProtected,
                CreatedDate = item.CreatedDate,
                UpdatedAt = item.UpdatedAt,
                SeoTitle = item.SeoTitle,
                SeoDescription = item.SeoDescription,
                SeoKeywords = item.SeoKeywords,
            })
            .ToListAsync(cancellationToken);

        return items;
    }
}
