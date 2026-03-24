namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbumById;

public class AdminAlbumDetailDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Genre { get; set; }

    public float Price { get; set; }

    public string PurchaseUrl { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public string? Media { get; set; }

    public string Label { get; set; } = string.Empty;

    public string Press { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastUpdateDate { get; set; }

    public string? Status { get; set; }

    public string? StockStatus { get; set; }

    public string SKU { get; set; } = string.Empty;

    public string? CanonicalTitle { get; set; }

    public int? OriginalYear { get; set; }

    public string? BandcampUrl { get; set; }

    public string Slug { get; set; } = string.Empty;

    public Guid BandId { get; set; }

    public string BandName { get; set; } = string.Empty;

    public Guid DistributorId { get; set; }

    public string DistributorName { get; set; } = string.Empty;
}
