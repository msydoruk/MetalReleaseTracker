using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class CatalogueDetailDto
{
    public Guid Id { get; set; }

    public Guid CatalogueIndexId { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public string BandName { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string SKU { get; set; } = string.Empty;

    public float Price { get; set; }

    public AlbumMediaType? Media { get; set; }

    public string? CanonicalTitle { get; set; }

    public int? OriginalYear { get; set; }

    public ChangeType ChangeType { get; set; }

    public PublicationStatus PublicationStatus { get; set; }

    public DateTime? LastPublishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
