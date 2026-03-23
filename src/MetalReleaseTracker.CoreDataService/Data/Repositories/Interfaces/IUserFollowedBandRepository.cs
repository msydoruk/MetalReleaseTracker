using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IUserFollowedBandRepository
{
    Task AddAsync(UserFollowedBandEntity entity, CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(string userId, Guid bandId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string userId, Guid bandId, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, bool>> GetFollowedBandIdsAsync(string userId, CancellationToken cancellationToken = default);

    Task<List<UserFollowedBandEntity>> GetFollowedBandsAsync(string userId, CancellationToken cancellationToken = default);

    Task<int> GetFollowerCountAsync(Guid bandId, CancellationToken cancellationToken = default);

    Task<PagedResultDto<AlbumEntity>> GetFeedAlbumsAsync(List<Guid> bandIds, int page, int pageSize, CancellationToken cancellationToken = default);
}
