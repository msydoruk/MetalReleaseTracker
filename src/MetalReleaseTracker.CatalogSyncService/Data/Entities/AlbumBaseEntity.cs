using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.CatalogSyncService.Data.Entities.Enums;

namespace MetalReleaseTracker.CatalogSyncService.Data.Entities;

public class AlbumBaseEntity
{
    public DistributorCode DistributorCode { get; set; }

    public string BandName { get; set; }

    public string SKU { get; set; }

    public string Name { get; set; }

    public DateTime ReleaseDate { get; set; }

    public string? Genre { get; set; }

    public float Price { get; set; }

    public string PurchaseUrl { get; set; }

    public string PhotoUrl { get; set; }

    public AlbumMediaType? Media { get; set; }

    public string Label { get; set; }

    public string Press { get; set; }

    public string? Description { get; set; }

    public AlbumStatus? Status { get; set; }

    public string? ParsedTitle { get; set; }

    public string? CanonicalTitle { get; set; }

    public int? OriginalYear { get; set; }
}