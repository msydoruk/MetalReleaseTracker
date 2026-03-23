using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IUserFollowedBandService
{
    Task FollowAsync(string userId, Guid bandId, CancellationToken cancellationToken = default);

    Task UnfollowAsync(string userId, Guid bandId, CancellationToken cancellationToken = default);

    Task<bool> IsFollowingAsync(string userId, Guid bandId, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, bool>> GetFollowedBandIdsAsync(string userId, CancellationToken cancellationToken = default);

    Task<List<BandDto>> GetFollowedBandsAsync(string userId, CancellationToken cancellationToken = default);

    Task<int> GetFollowerCountAsync(Guid bandId, CancellationToken cancellationToken = default);

    Task<PagedResultDto<AlbumDto>> GetFeedAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
