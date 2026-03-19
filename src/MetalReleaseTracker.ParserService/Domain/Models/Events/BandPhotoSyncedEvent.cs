namespace MetalReleaseTracker.ParserService.Domain.Models.Events;

public class BandPhotoSyncedEvent
{
    public string BandName { get; set; }

    public string PhotoBlobPath { get; set; }

    public string? Genre { get; set; }
}
