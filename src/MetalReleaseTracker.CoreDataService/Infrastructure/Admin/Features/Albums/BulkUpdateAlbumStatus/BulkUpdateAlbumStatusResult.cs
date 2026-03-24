namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.BulkUpdateAlbumStatus;

public class BulkUpdateAlbumStatusResult
{
    public bool InvalidStatus { get; set; }

    public int UpdatedCount { get; set; }
}
