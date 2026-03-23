using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IUserFavoriteRepository
{
    Task AddAsync(UserFavoriteEntity entity, CancellationToken cancellationToken = default);

    Task RemoveAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, UserCollectionStatus>> GetFavoriteAlbumIdsAsync(string userId, CancellationToken cancellationToken = default);

    Task<PagedResultDto<AlbumEntity>> GetFavoriteAlbumsAsync(string userId, int page, int pageSize, UserCollectionStatus? status = null, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(string userId, Guid albumId, UserCollectionStatus status, CancellationToken cancellationToken = default);

    Task<List<(UserFavoriteEntity Favorite, AlbumEntity Album)>> GetAllFavoriteAlbumsAsync(string userId, CancellationToken cancellationToken = default);
}
