using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBandById;

public class GetBandByIdHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetBandByIdHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminBandDetailDto?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var band = await _context.Bands
            .AsNoTracking()
            .Where(band => band.Id == id)
            .Select(band => new AdminBandDetailDto
            {
                Id = band.Id,
                Name = band.Name,
                Description = band.Description,
                Genre = band.Genre,
                PhotoUrl = band.PhotoUrl,
                MetalArchivesUrl = band.MetalArchivesUrl,
                FormationYear = band.FormationYear,
                Slug = band.Slug,
                IsVisible = band.IsVisible,
                SeoTitle = band.SeoTitle,
                SeoDescription = band.SeoDescription,
                SeoKeywords = band.SeoKeywords,
                AlbumCount = _context.Albums.Count(album => album.BandId == band.Id),
                Albums = _context.Albums
                    .Where(album => album.BandId == band.Id)
                    .Select(album => new AdminBandAlbumDto
                    {
                        Id = album.Id,
                        Name = album.Name,
                        SKU = album.SKU,
                        Price = album.Price,
                        Status = album.Status != null ? album.Status.ToString() : null,
                        DistributorName = album.Distributor.Name,
                    })
                    .OrderBy(album => album.Name)
                    .ToList(),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return band;
    }
}
