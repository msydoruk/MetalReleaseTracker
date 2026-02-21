using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class CatalogueIndexDto
{
    public Guid Id { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public string BandName { get; set; } = string.Empty;

    public string AlbumTitle { get; set; } = string.Empty;

    public string RawTitle { get; set; } = string.Empty;

    public string DetailUrl { get; set; } = string.Empty;

    public CatalogueIndexStatus Status { get; set; }

    public AlbumMediaType? MediaType { get; set; }

    public Guid? BandReferenceId { get; set; }

    public string? BandReferenceName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
