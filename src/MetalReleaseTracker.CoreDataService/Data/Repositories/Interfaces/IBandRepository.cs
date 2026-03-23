using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Seo;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IBandRepository
{
    Task<Guid> GetOrAddAsync(string bandName, CancellationToken cancellationToken = default);

    Task<List<BandEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<BandEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<BandWithAlbumCountDto>> GetBandsWithAlbumCountAsync(
        CancellationToken cancellationToken = default);

    Task<BandEntity?> GetByNameAsync(string bandName, CancellationToken cancellationToken = default);

    Task UpdateAsync(BandEntity band, CancellationToken cancellationToken = default);

    Task<List<string>> GetDistinctGenresAsync(CancellationToken cancellationToken = default);

    Task<List<BandEntity>> GetBandsByGenreAsync(string genre, Guid excludeBandId, int limit, CancellationToken cancellationToken = default);

    Task<BandEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<List<BandSitemapDto>> GetAllBandSlugsAsync(CancellationToken cancellationToken = default);
}