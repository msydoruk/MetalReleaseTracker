using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Domain.Models.Events;

public class AlbumParsedPublicationEvent
{
    public DateTime CreatedDate { get; set; }

    public Guid ParsingSessionId { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public List<string> StorageFilePaths { get; set; }
}