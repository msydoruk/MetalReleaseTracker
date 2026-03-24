using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.BulkUpdateAlbumStatus;

public class BulkUpdateAlbumStatusHandler
{
    private readonly CoreDataServiceDbContext _context;

    public BulkUpdateAlbumStatusHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<BulkUpdateAlbumStatusResult> HandleAsync(
        BulkUpdateAlbumStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<AlbumStockStatus>(request.StockStatus, ignoreCase: true, out var stockStatus))
        {
            return new BulkUpdateAlbumStatusResult { InvalidStatus = true };
        }

        var albums = await _context.Albums
            .Where(album => request.AlbumIds.Contains(album.Id))
            .ToListAsync(cancellationToken);

        foreach (var album in albums)
        {
            album.StockStatus = stockStatus;
            album.LastUpdateDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new BulkUpdateAlbumStatusResult
        {
            UpdatedCount = albums.Count,
        };
    }
}
