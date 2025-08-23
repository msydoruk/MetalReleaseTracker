using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IDistributorsRepository
{
    Task<Guid> GetOrAddAsync(string distributorName, CancellationToken cancellationToken = default);

    Task<List<DistributorEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<DistributorEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<DistributorWithAlbumCountDto>> GetDistributorsWithAlbumCountAsync(
        CancellationToken cancellationToken = default);
}