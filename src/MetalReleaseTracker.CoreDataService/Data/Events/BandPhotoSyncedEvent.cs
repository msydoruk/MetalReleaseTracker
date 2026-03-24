namespace MetalReleaseTracker.CoreDataService.Data.Events;

public class BandPhotoSyncedEvent
{
    public string BandName { get; set; }

    public string PhotoBlobPath { get; set; }

    public string? Genre { get; set; }

    public int? FormationYear { get; set; }

    public string? Description { get; set; }
}
