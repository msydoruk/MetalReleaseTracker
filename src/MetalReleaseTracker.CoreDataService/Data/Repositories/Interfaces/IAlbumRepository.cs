using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IAlbumRepository
{
    Task<AlbumEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResultDto<AlbumEntity>> GetFilteredAlbumsAsync(AlbumFilterDto filter,
        CancellationToken cancellationToken = default);

    Task<List<AlbumEntity>> GetAllFilteredAlbumsAsync(AlbumFilterDto filter,
        CancellationToken cancellationToken = default);

    Task AddAsync(AlbumEntity entity, CancellationToken cancellationToken = default);

    Task UpdateAsync(AlbumEntity entity, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}