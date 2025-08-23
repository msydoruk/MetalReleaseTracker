using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MetalReleaseTracker.CatalogSyncService.Data.Entities;

public class RawAlbumEntity : AlbumBaseEntity
{
    public RawAlbumEntity()
    {
    }

    public RawAlbumEntity(Guid id)
    {
        Id = id;
    }

    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    [BsonRepresentation(BsonType.String)]
    public Guid ParsingSessionId { get; set; }
}
