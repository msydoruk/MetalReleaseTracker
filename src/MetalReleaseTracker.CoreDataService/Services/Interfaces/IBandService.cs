using MetalReleaseTracker.CoreDataService.Services.Dtos;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IBandService
{
    Task<List<BandDto>> GetAllBandsAsync(CancellationToken cancellationToken = default);

    Task<BandDto?> GetBandByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<BandWithAlbumCountDto>> GetBandsWithAlbumCountAsync(CancellationToken cancellationToken = default);
}