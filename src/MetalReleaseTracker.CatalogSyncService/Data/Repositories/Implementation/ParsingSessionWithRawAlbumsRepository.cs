using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MetalReleaseTracker.CatalogSyncService.Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace MetalReleaseTracker.CatalogSyncService.Data.Repositories.Implementation;

public class ParsingSessionWithRawAlbumsRepository : IParsingSessionWithRawAlbumsRepository
{
    private readonly IMongoCollection<ParsingSessionWithRawAlbumsEntity> _parsingSessionWithRawAlbumsCollection;

    public ParsingSessionWithRawAlbumsRepository(IMongoDatabase database, IOptions<MongoDbConfig> mongoDbConfig)
    {
        _parsingSessionWithRawAlbumsCollection =
            database.GetCollection<ParsingSessionWithRawAlbumsEntity>(mongoDbConfig.Value
                .ParsingSessionWithRawAlbumsCollectionName);
    }

    public async Task<List<ParsingSessionWithRawAlbumsEntity>> GetUnProcessedAsync()
    {
        var filter = Builders<ParsingSessionWithRawAlbumsEntity>.Filter
            .In(album => album.ProcessingStatus, [
                ParsingSessionProcessingStatus.Pending,
                ParsingSessionProcessingStatus.Failed
            ]);
        var order = Builders<ParsingSessionWithRawAlbumsEntity>.Sort.Ascending(album => album.CreatedDate);

        return await _parsingSessionWithRawAlbumsCollection.Find(filter).Sort(order).ToListAsync();
    }

    public async Task AddAsync(ParsingSessionWithRawAlbumsEntity parsingSessionEntity)
    {
        await _parsingSessionWithRawAlbumsCollection.InsertOneAsync(parsingSessionEntity);
    }

    public async Task UpdateProcessingStatusAsync(Guid id, ParsingSessionProcessingStatus processingStatus)
    {
        var filter = Builders<ParsingSessionWithRawAlbumsEntity>.Filter.Eq(album => album.Id, id);

        var updates = new List<UpdateDefinition<ParsingSessionWithRawAlbumsEntity>>
        {
            Builders<ParsingSessionWithRawAlbumsEntity>.Update.Set(album => album.ProcessingStatus, processingStatus),
            Builders<ParsingSessionWithRawAlbumsEntity>.Update.Set(album => album.LastUpdateDate, DateTime.UtcNow)
        };

        if (processingStatus == ParsingSessionProcessingStatus.Processed)
        {
            updates.Add(Builders<ParsingSessionWithRawAlbumsEntity>.Update.Set(album => album.ProcessedDate, DateTime.UtcNow));
        }

        var updateDefinition = Builders<ParsingSessionWithRawAlbumsEntity>.Update.Combine(updates);

        await _parsingSessionWithRawAlbumsCollection.UpdateOneAsync(filter, updateDefinition);
    }
}