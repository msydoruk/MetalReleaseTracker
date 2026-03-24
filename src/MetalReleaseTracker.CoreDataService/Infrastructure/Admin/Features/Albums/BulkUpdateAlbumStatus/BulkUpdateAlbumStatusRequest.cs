namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.BulkUpdateAlbumStatus;

public class BulkUpdateAlbumStatusRequest
{
    public List<Guid> AlbumIds { get; set; } = [];

    public string StockStatus { get; set; } = string.Empty;
}
