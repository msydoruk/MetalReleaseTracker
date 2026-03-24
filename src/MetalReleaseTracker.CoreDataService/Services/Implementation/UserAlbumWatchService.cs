using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class UserAlbumWatchService : IUserAlbumWatchService
{
    private readonly IUserAlbumWatchRepository _userAlbumWatchRepository;
    private readonly IAlbumRepository _albumRepository;

    public UserAlbumWatchService(
        IUserAlbumWatchRepository userAlbumWatchRepository,
        IAlbumRepository albumRepository)
    {
        _userAlbumWatchRepository = userAlbumWatchRepository;
        _albumRepository = albumRepository;
    }

    public async Task WatchAlbumAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        var album = await _albumRepository.GetAsync(albumId, cancellationToken);
        if (album == null)
        {
            return;
        }

        var canonicalTitle = album.CanonicalTitle ?? album.Name;
        var exists = await _userAlbumWatchRepository.ExistsAsync(userId, album.BandId, canonicalTitle, cancellationToken);
        if (exists)
        {
            return;
        }

        var entity = new UserAlbumWatchEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AlbumId = albumId,
            CanonicalTitle = canonicalTitle,
            BandId = album.BandId,
            CreatedDate = DateTime.UtcNow
        };

        await _userAlbumWatchRepository.AddAsync(entity, cancellationToken);
    }

    public async Task UnwatchAlbumAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        var album = await _albumRepository.GetAsync(albumId, cancellationToken);
        if (album == null)
        {
            return;
        }

        var canonicalTitle = album.CanonicalTitle ?? album.Name;
        await _userAlbumWatchRepository.RemoveAsync(userId, album.BandId, canonicalTitle, cancellationToken);
    }

    public async Task<bool> IsWatchingAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        var album = await _albumRepository.GetAsync(albumId, cancellationToken);
        if (album == null)
        {
            return false;
        }

        var canonicalTitle = album.CanonicalTitle ?? album.Name;
        return await _userAlbumWatchRepository.ExistsAsync(userId, album.BandId, canonicalTitle, cancellationToken);
    }

    public async Task<Dictionary<string, bool>> GetWatchedKeysAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _userAlbumWatchRepository.GetWatchedKeysAsync(userId, cancellationToken);
    }
}
