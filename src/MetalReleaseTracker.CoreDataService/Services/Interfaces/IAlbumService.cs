using MetalReleaseTracker.CoreDataService.Services.Dtos;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IAlbumService
{
    Task<PagedResultDto<AlbumDto>> GetFilteredAlbums(AlbumFilterDto filter, CancellationToken cancellationToken = default);

    Task<AlbumDto?> GetAlbumById(Guid id, CancellationToken cancellationToken = default);
}