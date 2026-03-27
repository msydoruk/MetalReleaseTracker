namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetTopDistributors;

public class DistributorAlbumCountDto
{
    public Guid DistributorId { get; set; }

    public string DistributorName { get; set; }

    public int AlbumCount { get; set; }
}
