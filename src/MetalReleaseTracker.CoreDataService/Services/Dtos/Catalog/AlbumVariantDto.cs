namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class AlbumVariantDto
{
    public Guid AlbumId { get; set; }

    public Guid DistributorId { get; set; }

    public string DistributorName { get; set; } = string.Empty;

    public float Price { get; set; }

    public string PurchaseUrl { get; set; } = string.Empty;
}
