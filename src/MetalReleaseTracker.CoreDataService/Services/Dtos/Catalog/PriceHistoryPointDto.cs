namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class PriceHistoryPointDto
{
    public float Price { get; set; }

    public float? OldPrice { get; set; }

    public string ChangeType { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string DistributorName { get; set; } = string.Empty;
}
