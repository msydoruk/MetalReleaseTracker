using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MetalReleaseTracker.CatalogSyncService.Data.Entities;

public class AlbumProcessedEntity : AlbumBaseEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public AlbumProcessedStatus ProcessedStatus { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastUpdateDate { get; set; }

    public DateTime? LastCheckedDate { get; set; }

    public DateTime? LastPublishedDate { get; set; }

    public Dictionary<string, object> GetChangedFields(AlbumBaseEntity other)
    {
        var changedFields = new Dictionary<string, object>();

        if (Price != other.Price) changedFields.Add(nameof(other.Price), other.Price);
        if (Status != other.Status) changedFields.Add(nameof(other.Status), other.Status);

        return changedFields;
    }
}
