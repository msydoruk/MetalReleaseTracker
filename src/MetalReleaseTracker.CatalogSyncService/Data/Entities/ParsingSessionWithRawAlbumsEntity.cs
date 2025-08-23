using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MetalReleaseTracker.CatalogSyncService.Data.Entities;

public class ParsingSessionWithRawAlbumsEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public ParsingSessionProcessingStatus ProcessingStatus { get; set; } = ParsingSessionProcessingStatus.Pending;

    public DateTime CreatedDate { get; set; }

    public DateTime? ProcessedDate { get; set; }

    public DateTime? LastUpdateDate { get; set; }

    public List<RawAlbumEntity> RawAlbums { get; set; }
}