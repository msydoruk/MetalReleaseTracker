using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IAlbumRatingService
{
    Task SubmitRatingAsync(string userId, Guid albumId, int rating, CancellationToken cancellationToken = default);

    Task<AlbumRatingDto> GetRatingAsync(Guid albumId, string? userId = null, CancellationToken cancellationToken = default);

    Task DeleteRatingAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);
}
