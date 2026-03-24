namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBandById;

public class AdminBandAlbumDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public float Price { get; set; }

    public string? Status { get; set; }

    public string DistributorName { get; set; } = string.Empty;
}
