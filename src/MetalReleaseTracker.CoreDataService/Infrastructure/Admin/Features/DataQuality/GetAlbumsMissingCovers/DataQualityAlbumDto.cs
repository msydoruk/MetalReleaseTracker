namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetAlbumsMissingCovers;

public class DataQualityAlbumDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public string BandName { get; set; } = string.Empty;

    public string DistributorName { get; set; } = string.Empty;

    public string? PhotoUrl { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
