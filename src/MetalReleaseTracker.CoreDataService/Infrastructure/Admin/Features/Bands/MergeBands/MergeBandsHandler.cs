using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBandById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.MergeBands;

public class MergeBandsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public MergeBandsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<MergeBandsResult> HandleAsync(
        MergeBandsRequest request,
        CancellationToken cancellationToken = default)
    {
        var targetBand = await _context.Bands
            .FirstOrDefaultAsync(
                band => band.Id == request.TargetBandId,
                cancellationToken);

        if (targetBand is null)
        {
            return new MergeBandsResult { NotFound = true };
        }

        var sourceBand = await _context.Bands
            .FirstOrDefaultAsync(
                band => band.Id == request.SourceBandId,
                cancellationToken);

        if (sourceBand is null)
        {
            return new MergeBandsResult { NotFound = true };
        }

        var sourceAlbums = await _context.Albums
            .Where(album => album.BandId == request.SourceBandId)
            .ToListAsync(cancellationToken);

        foreach (var album in sourceAlbums)
        {
            album.BandId = request.TargetBandId;
        }

        _context.Bands.Remove(sourceBand);
        await _context.SaveChangesAsync(cancellationToken);

        var band = await _context.Bands
            .AsNoTracking()
            .Include(band => band.Translations)
            .FirstOrDefaultAsync(band => band.Id == request.TargetBandId, cancellationToken);

        if (band is null)
        {
            return new MergeBandsResult { NotFound = true };
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

        var detail = new AdminBandDetailDto
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

        return new MergeBandsResult { Detail = detail };
    }
}
