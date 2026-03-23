using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IUserFavoriteService
{
    Task AddFavoriteAsync(string userId, Guid albumId, UserCollectionStatus status = UserCollectionStatus.Favorite, CancellationToken cancellationToken = default);

    Task RemoveFavoriteAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<bool> IsFavoriteAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, int>> GetFavoriteIdsAsync(string userId, CancellationToken cancellationToken = default);

    Task<PagedResultDto<AlbumDto>> GetFavoriteAlbumsAsync(string userId, int page, int pageSize, UserCollectionStatus? status = null, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(string userId, Guid albumId, UserCollectionStatus status, CancellationToken cancellationToken = default);

    Task<byte[]> ExportCollectionAsync(string userId, string format, CancellationToken cancellationToken = default);
}
