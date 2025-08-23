using MetalReleaseTracker.ParserService.Domain.Models.Entities;

namespace MetalReleaseTracker.ParserService.Domain.Models.Events;

public class AlbumParsedEvent : AlbumBase
{
    public DateTime CreatedDate { get; set; }
}