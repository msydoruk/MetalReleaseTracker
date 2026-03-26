using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;
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
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .Include(album => album.Translations)
            .FirstOrDefaultAsync(album => album.Id == id, cancellationToken);

        if (album is null)
        {
            return null;
        }

        return new AdminAlbumDetailDto
        {
            Id = album.Id,
            Name = album.Name,
            Genre = album.Genre,
            Price = album.Price,
            PurchaseUrl = album.PurchaseUrl,
            PhotoUrl = album.PhotoUrl,
            Media = album.Media?.ToString(),
            Label = album.Label,
            Press = album.Press,
            Description = album.Description,
            CreatedDate = album.CreatedDate,
            LastUpdateDate = album.LastUpdateDate,
            Status = album.Status?.ToString(),
            StockStatus = album.StockStatus?.ToString(),
            SKU = album.SKU,
            CanonicalTitle = album.CanonicalTitle,
            OriginalYear = album.OriginalYear,
            BandcampUrl = album.BandcampUrl,
            Slug = album.Slug,
            BandId = album.BandId,
            BandName = album.Band.Name,
            DistributorId = album.DistributorId,
            DistributorName = album.Distributor.Name,
            Translations = album.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new AlbumTranslationDto
                {
                    SeoTitle = translation.SeoTitle,
                    SeoDescription = translation.SeoDescription,
                    SeoKeywords = translation.SeoKeywords,
                }),
        };
    }
}
