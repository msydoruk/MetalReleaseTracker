using MetalReleaseTracker.CatalogSyncService.Configurations;

namespace MetalReleaseTracker.CatalogSyncService.Data.Events;

public class AlbumParsedPublicationEvent
{
    public DateTime CreatedDate { get; set; }

    public Guid ParsingSessionId { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public List<string> StorageFilePaths { get; set; }
}