using MetalReleaseTracker.CoreDataService.Data.Entities;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IUserAlbumWatchRepository
{
    Task AddAsync(UserAlbumWatchEntity entity, CancellationToken cancellationToken = default);

    Task RemoveAsync(string userId, Guid bandId, string? canonicalTitle, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string userId, Guid bandId, string? canonicalTitle, CancellationToken cancellationToken = default);

    Task<Dictionary<string, bool>> GetWatchedKeysAsync(string userId, CancellationToken cancellationToken = default);

    Task<List<string>> GetWatcherUserIdsAsync(Guid bandId, string? canonicalTitle, CancellationToken cancellationToken = default);
}
