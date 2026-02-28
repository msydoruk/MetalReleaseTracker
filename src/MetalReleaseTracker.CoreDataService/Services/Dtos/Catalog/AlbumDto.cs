using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class AlbumDto
{
    public Guid Id { get; set; }

    public Guid DistributorId { get; set; }

    public string DistributorName { get; set; } = string.Empty;

    public Guid BandId { get; set; }

    public string BandName { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime ReleaseDate { get; set; }

    public string? Genre { get; set; }

    public float Price { get; set; }

    public string PurchaseUrl { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public AlbumMediaType? Media { get; set; }

    public string Label { get; set; } = string.Empty;

    public string Press { get; set; } = string.Empty;

    public string? Description { get; set; }

    public AlbumStatus? Status { get; set; }

    public string? CanonicalTitle { get; set; }

    public int? OriginalYear { get; set; }
}