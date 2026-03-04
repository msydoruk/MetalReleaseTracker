using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IAlbumChangeLogRepository
{
    Task AddAsync(AlbumChangeLogEntity entity, CancellationToken cancellationToken = default);

    Task<PagedResultDto<AlbumChangeLogEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
