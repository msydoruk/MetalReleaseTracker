using MetalReleaseTracker.CoreDataService.Services.Dtos;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IDistributorService
{
    Task<List<DistributorDto>> GetAllDistributorsAsync(CancellationToken cancellationToken = default);

    Task<DistributorDto?> GetDistributorByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<DistributorWithAlbumCountDto>> GetDistributorsWithAlbumCountAsync(
        CancellationToken cancellationToken = default);
}
