using System.ComponentModel.DataAnnotations;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Domain.Models.Entities;

public class CatalogueIndexEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public DistributorCode DistributorCode { get; set; }

    [Required]
    public string BandName { get; set; }

    public string AlbumTitle { get; set; }

    public Guid? BandDiscographyId { get; set; }

    public BandDiscographyEntity? BandDiscography { get; set; }

    public string RawTitle { get; set; }

    [Required]
    public string DetailUrl { get; set; }

    [Required]
    public CatalogueIndexStatus Status { get; set; }

    public AlbumMediaType? MediaType { get; set; }

    public Guid? BandReferenceId { get; set; }

    public BandReferenceEntity? BandReference { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
