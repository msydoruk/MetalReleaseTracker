using MetalReleaseTracker.CatalogSyncService.Data.Entities;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MongoDB.Driver;

namespace MetalReleaseTracker.CatalogSyncService.Data.Repositories.Interfaces;

public interface IParsingSessionWithRawAlbumsRepository
{
    Task<List<ParsingSessionWithRawAlbumsEntity>> GetUnProcessedAsync();

    Task AddAsync(ParsingSessionWithRawAlbumsEntity parsingSessionEntity);

    Task UpdateProcessingStatusAsync(Guid id, ParsingSessionProcessingStatus processingStatus);
}