using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.DeleteBand;

public class DeleteBandHandler
{
    private readonly CoreDataServiceDbContext _context;

    public DeleteBandHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Bands
            .FirstOrDefaultAsync(
                band => band.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _context.Bands.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
