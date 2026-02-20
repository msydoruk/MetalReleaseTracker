using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IUserFavoriteRepository
{
    Task AddAsync(UserFavoriteEntity entity, CancellationToken cancellationToken = default);

    Task RemoveAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetFavoriteAlbumIdsAsync(string userId, CancellationToken cancellationToken = default);

    Task<PagedResultDto<AlbumEntity>> GetFavoriteAlbumsAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
