using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;
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
            .Include(band => band.Translations)
            .FirstOrDefaultAsync(band => band.Id == id, cancellationToken);

        if (band is null)
        {
            return null;
        }

        var albumCount = await _context.Albums
            .CountAsync(album => album.BandId == band.Id, cancellationToken);

        var albums = await _context.Albums
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
            .ToListAsync(cancellationToken);

        return new AdminBandDetailDto
        {
            Id = band.Id,
            Name = band.Name,
            Genre = band.Genre,
            PhotoUrl = band.PhotoUrl,
            MetalArchivesUrl = band.MetalArchivesUrl,
            FormationYear = band.FormationYear,
            Slug = band.Slug,
            IsVisible = band.IsVisible,
            AlbumCount = albumCount,
            Albums = albums,
            Translations = band.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new BandTranslationDto
                {
                    Description = translation.Description,
                    SeoTitle = translation.SeoTitle,
                    SeoDescription = translation.SeoDescription,
                    SeoKeywords = translation.SeoKeywords,
                }),
        };
    }
}
