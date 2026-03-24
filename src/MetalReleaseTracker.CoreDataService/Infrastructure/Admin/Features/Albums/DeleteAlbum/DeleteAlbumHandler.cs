using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.DeleteAlbum;

public class DeleteAlbumHandler
{
    private readonly CoreDataServiceDbContext _context;

    public DeleteAlbumHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Albums
            .FirstOrDefaultAsync(
                album => album.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _context.Albums.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
