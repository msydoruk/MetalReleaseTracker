using MetalReleaseTracker.CoreDataService.Configuration;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;

namespace MetalReleaseTracker.CoreDataService.Data.Events;

public class AlbumProcessedPublicationEvent
{
    public Guid Id { get; set; }

    public AlbumProcessedStatus ProcessedStatus { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? LastUpdateDate { get; set; }

    public DateTime? LastCheckedDate { get; set; }

    public DateTime? LastPublishedDate { get; set; }

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