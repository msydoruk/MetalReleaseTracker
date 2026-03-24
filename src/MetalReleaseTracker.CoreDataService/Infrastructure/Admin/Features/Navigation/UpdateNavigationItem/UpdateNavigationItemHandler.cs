using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.UpdateNavigationItem;

public class UpdateNavigationItemHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateNavigationItemHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<NavigationItemDto?> HandleAsync(
        Guid id,
        UpdateNavigationItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.NavigationItems
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (request.TitleEn is not null)
        {
            entity.TitleEn = request.TitleEn;
        }

        if (request.TitleUa is not null)
        {
            entity.TitleUa = request.TitleUa;
        }

        if (request.Path is not null)
        {
            entity.Path = request.Path;
        }

        if (request.IconName is not null)
        {
            entity.IconName = request.IconName;
        }

        if (request.SortOrder.HasValue)
        {
            entity.SortOrder = request.SortOrder.Value;
        }

        if (request.IsVisible.HasValue)
        {
            entity.IsVisible = request.IsVisible.Value;
        }

        if (request.IsProtected.HasValue)
        {
            entity.IsProtected = request.IsProtected.Value;
        }

        entity.UpdatedAt = DateTime.UtcNow;
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
        };
    }
}
