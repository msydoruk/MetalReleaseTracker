using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBandById;
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

        var detail = await _context.Bands
            .AsNoTracking()
            .Where(band => band.Id == request.TargetBandId)
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

        return new MergeBandsResult { Detail = detail };
    }
}
