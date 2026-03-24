namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IUserAlbumWatchService
{
    Task WatchAlbumAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task UnwatchAlbumAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<bool> IsWatchingAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<Dictionary<string, bool>> GetWatchedKeysAsync(string userId, CancellationToken cancellationToken = default);
}
