using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.DeleteDistributor;

public class DeleteDistributorHandler
{
    private readonly CoreDataServiceDbContext _context;

    public DeleteDistributorHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Distributors
            .FirstOrDefaultAsync(
                distributor => distributor.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _context.Distributors.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
