using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbumById;

public class GetAlbumByIdHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetAlbumByIdHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminAlbumDetailDto?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var album = await _context.Albums
            .AsNoTracking()
            .Where(album => album.Id == id)
            .Select(album => new AdminAlbumDetailDto
            {
                Id = album.Id,
                Name = album.Name,
                Genre = album.Genre,
                Price = album.Price,
                PurchaseUrl = album.PurchaseUrl,
                PhotoUrl = album.PhotoUrl,
                Media = album.Media != null ? album.Media.ToString() : null,
                Label = album.Label,
                Press = album.Press,
                Description = album.Description,
                CreatedDate = album.CreatedDate,
                LastUpdateDate = album.LastUpdateDate,
                Status = album.Status != null ? album.Status.ToString() : null,
                StockStatus = album.StockStatus != null ? album.StockStatus.ToString() : null,
                SKU = album.SKU,
                CanonicalTitle = album.CanonicalTitle,
                OriginalYear = album.OriginalYear,
                BandcampUrl = album.BandcampUrl,
                Slug = album.Slug,
                BandId = album.BandId,
                BandName = album.Band.Name,
                DistributorId = album.DistributorId,
                DistributorName = album.Distributor.Name,
                SeoTitle = album.SeoTitle,
                SeoDescription = album.SeoDescription,
                SeoKeywords = album.SeoKeywords,
            })
            .FirstOrDefaultAsync(cancellationToken);

        return album;
    }
}
