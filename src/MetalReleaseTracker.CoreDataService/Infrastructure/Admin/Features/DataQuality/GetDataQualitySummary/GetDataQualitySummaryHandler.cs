using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetDataQualitySummary;

public class GetDataQualitySummaryHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetDataQualitySummaryHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<DataQualitySummaryDto> HandleAsync(CancellationToken cancellationToken = default)
    {
        var albumsMissingCoversTask = _context.Albums
            .AsNoTracking()
            .CountAsync(album => album.PhotoUrl == null || album.PhotoUrl == string.Empty, cancellationToken);

        var bandsMissingGenreTask = _context.Bands
            .AsNoTracking()
            .CountAsync(band => band.Genre == null || band.Genre == string.Empty, cancellationToken);

        var bandsMissingPhotoTask = _context.Bands
            .AsNoTracking()
            .CountAsync(band => band.PhotoUrl == null || band.PhotoUrl == string.Empty, cancellationToken);

        var potentialDuplicatesTask = _context.Bands
            .AsNoTracking()
            .GroupBy(band => band.Name.ToLower().Trim())
            .Where(group => group.Count() > 1)
            .CountAsync(cancellationToken);

        await Task.WhenAll(albumsMissingCoversTask, bandsMissingGenreTask, bandsMissingPhotoTask, potentialDuplicatesTask);

        return new DataQualitySummaryDto
        {
            AlbumsMissingCovers = albumsMissingCoversTask.Result,
            BandsMissingGenre = bandsMissingGenreTask.Result,
            BandsMissingPhoto = bandsMissingPhotoTask.Result,
            PotentialDuplicateBands = potentialDuplicatesTask.Result,
        };
    }
}
