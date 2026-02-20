using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IUserFavoriteService
{
    Task AddFavoriteAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task RemoveFavoriteAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<bool> IsFavoriteAsync(string userId, Guid albumId, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetFavoriteIdsAsync(string userId, CancellationToken cancellationToken = default);

    Task<PagedResultDto<AlbumDto>> GetFavoriteAlbumsAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
