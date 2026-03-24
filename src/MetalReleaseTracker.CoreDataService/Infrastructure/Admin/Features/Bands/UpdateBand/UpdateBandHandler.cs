using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.UpdateBand;

public class UpdateBandHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateBandHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminBandDto?> HandleAsync(
        Guid id,
        UpdateBandRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Bands
            .FirstOrDefaultAsync(
                band => band.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (request.Name is not null)
        {
            entity.Name = request.Name;
        }

        if (request.Description is not null)
        {
            entity.Description = request.Description;
        }

        if (request.Genre is not null)
        {
            entity.Genre = request.Genre;
        }

        if (request.PhotoUrl is not null)
        {
            entity.PhotoUrl = request.PhotoUrl;
        }

        if (request.MetalArchivesUrl is not null)
        {
            entity.MetalArchivesUrl = request.MetalArchivesUrl;
        }

        if (request.FormationYear is not null)
        {
            entity.FormationYear = request.FormationYear;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var albumCount = await _context.Albums
            .CountAsync(
                album => album.BandId == entity.Id,
                cancellationToken);

        return new AdminBandDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Genre = entity.Genre,
            PhotoUrl = entity.PhotoUrl,
            MetalArchivesUrl = entity.MetalArchivesUrl,
            FormationYear = entity.FormationYear,
            AlbumCount = albumCount,
            Slug = entity.Slug,
        };
    }
}
