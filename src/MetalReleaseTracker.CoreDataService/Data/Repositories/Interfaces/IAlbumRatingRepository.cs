using MetalReleaseTracker.CoreDataService.Data.Entities;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IAlbumRatingRepository
{
    Task<AlbumRatingEntity?> GetAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task AddOrUpdateAsync(AlbumRatingEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<double?> GetAverageRatingAsync(Guid albumId, CancellationToken cancellationToken = default);

    Task<int> GetRatingCountAsync(Guid albumId, CancellationToken cancellationToken = default);
}
