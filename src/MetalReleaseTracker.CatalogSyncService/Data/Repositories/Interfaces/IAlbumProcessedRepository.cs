using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MongoDB.Driver;

namespace MetalReleaseTracker.CatalogSyncService.Data.Repositories.Interfaces;

public interface IAlbumProcessedRepository
{
    Task<AlbumProcessedEntity?> GetBySkuAsync(string sku, CancellationToken cancellationToken);

    Task<List<AlbumProcessedEntity>> GetUnPublishedBatchAsync(int batchSize, CancellationToken cancellationToken);

    Task<List<string>> GetSkusByDistributorAsync(DistributorCode distributorCode, CancellationToken cancellationToken);

    Task AddAsync(AlbumProcessedEntity albumProcessedEntity, CancellationToken cancellationToken);

    Task UpdateAsync(Guid id, Dictionary<string, object> changedFields, CancellationToken cancellationToken);

    Task UpdateStatusBySkuAsync(string sku, AlbumProcessedStatus albumProcessedStatus, CancellationToken cancellationToken);
}