namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class AlbumChangeLogDto
{
    public Guid Id { get; set; }

    public string AlbumName { get; set; }

    public string BandName { get; set; }

    public string DistributorName { get; set; }

    public float Price { get; set; }

    public string? PurchaseUrl { get; set; }

    public string ChangeType { get; set; }

    public DateTime ChangedAt { get; set; }
}
