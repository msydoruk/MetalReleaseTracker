using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.HideAlbum;

public class HideAlbumHandler
{
    private readonly CoreDataServiceDbContext _context;

    public HideAlbumHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var album = await _context.Albums
            .FirstOrDefaultAsync(album => album.Id == id, cancellationToken);

        if (album is null)
        {
            return false;
        }

        album.Status = AlbumStatus.Unavailable;
        album.StockStatus = AlbumStockStatus.OutOfStock;
        album.LastUpdateDate = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
