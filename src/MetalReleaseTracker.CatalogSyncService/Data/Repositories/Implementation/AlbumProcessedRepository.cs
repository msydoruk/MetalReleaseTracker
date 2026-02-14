using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MetalReleaseTracker.CatalogSyncService.Data.Repositories.Implementation;

public class AlbumProcessedRepository : IAlbumProcessedRepository
{
    private readonly IMongoCollection<AlbumProcessedEntity> _processedAlbumsCollection;

    public AlbumProcessedRepository(IMongoDatabase database, IOptions<MongoDbConfig> mongoDbConfig)
    {
        _processedAlbumsCollection =
            database.GetCollection<AlbumProcessedEntity>(mongoDbConfig.Value.ProcessedAlbumsCollectionName);
    }

    public async Task<AlbumProcessedEntity?> GetBySkuAsync(string sku, CancellationToken cancellationToken)
    {
        var filter = Builders<AlbumProcessedEntity>.Filter.Eq(album => album.SKU, sku);

        return await _processedAlbumsCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<AlbumProcessedEntity>> GetUnPublishedBatchAsync(int batchSize, CancellationToken cancellationToken)
    {
        var filter = Builders<AlbumProcessedEntity>.Filter.Nin(album => album.ProcessedStatus, new[]
        {
            AlbumProcessedStatus.Published
        });

        return await _processedAlbumsCollection.Find(filter).Limit(batchSize).ToListAsync(cancellationToken);
    }

    public async Task<List<string>> GetSkusByDistributorAsync(DistributorCode distributorCode, CancellationToken cancellationToken)
    {
        var filter = Builders<AlbumProcessedEntity>.Filter.Eq(album => album.DistributorCode, distributorCode);
        var projection = Builders<AlbumProcessedEntity>.Projection.Expression(album => album.SKU);

        return await _processedAlbumsCollection.Find(filter).Project(projection).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AlbumProcessedEntity albumProcessedEntity, CancellationToken cancellationToken)
    {
        await _processedAlbumsCollection.InsertOneAsync(albumProcessedEntity, null, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, Dictionary<string, object> changedFields, CancellationToken cancellationToken)
    {
        var filter = Builders<AlbumProcessedEntity>.Filter.Eq(album => album.Id, id);
        var updateDefinition = Builders<AlbumProcessedEntity>.Update.Combine(
            changedFields.Select(field => Builders<AlbumProcessedEntity>.Update.Set(field.Key, field.Value)));

        await _processedAlbumsCollection.UpdateOneAsync(filter, updateDefinition, null, cancellationToken);
    }

    public async Task UpdateStatusBySkuAsync(string sku, AlbumProcessedStatus albumProcessedStatus, CancellationToken cancellationToken)
    {
        var filer = Builders<AlbumProcessedEntity>.Filter.Eq(album => album.SKU, sku);
        var updateDefinition = Builders<AlbumProcessedEntity>.Update.Set(album => album.ProcessedStatus, albumProcessedStatus);

        await _processedAlbumsCollection.UpdateOneAsync(filer, updateDefinition, null, cancellationToken);
    }
}