using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.CreateNavigationItem;

public class CreateNavigationItemHandler
{
    private readonly CoreDataServiceDbContext _context;

    public CreateNavigationItemHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<NavigationItemDto> HandleAsync(
        CreateNavigationItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new NavigationItemEntity
        {
            Id = Guid.NewGuid(),
            TitleEn = request.TitleEn,
            TitleUa = request.TitleUa,
            Path = request.Path,
            IconName = request.IconName,
            SortOrder = request.SortOrder,
            IsVisible = request.IsVisible,
            IsProtected = request.IsProtected,
            CreatedDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            SeoTitle = request.SeoTitle,
            SeoDescription = request.SeoDescription,
            SeoKeywords = request.SeoKeywords,
        };

        _context.NavigationItems.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new NavigationItemDto
        {
            Id = entity.Id,
            TitleEn = entity.TitleEn,
            TitleUa = entity.TitleUa,
            Path = entity.Path,
            IconName = entity.IconName,
            SortOrder = entity.SortOrder,
            IsVisible = entity.IsVisible,
            IsProtected = entity.IsProtected,
            CreatedDate = entity.CreatedDate,
            UpdatedAt = entity.UpdatedAt,
            SeoTitle = entity.SeoTitle,
            SeoDescription = entity.SeoDescription,
            SeoKeywords = entity.SeoKeywords,
        };
    }
}
