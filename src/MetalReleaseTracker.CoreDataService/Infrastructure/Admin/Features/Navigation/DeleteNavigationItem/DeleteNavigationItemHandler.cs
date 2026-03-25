using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.DeleteNavigationItem;

public class DeleteNavigationItemHandler
{
    private readonly CoreDataServiceDbContext _context;

    public DeleteNavigationItemHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.NavigationItems
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _context.NavigationItems.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
