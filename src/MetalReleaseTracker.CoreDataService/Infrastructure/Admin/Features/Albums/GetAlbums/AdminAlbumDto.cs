namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;

public class AdminAlbumDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string BandName { get; set; } = string.Empty;

    public string DistributorName { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public float Price { get; set; }

    public string? Status { get; set; }

    public string? StockStatus { get; set; }

    public string? Media { get; set; }

    public DateTime CreatedDate { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }
}
