using System.ComponentModel.DataAnnotations;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Domain.Models.Entities;

public class CatalogueIndexDetailEntity : AlbumBase
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CatalogueIndexId { get; set; }

    public CatalogueIndexEntity CatalogueIndex { get; set; }

    [Required]
    public ChangeType ChangeType { get; set; }

    [Required]
    public PublicationStatus PublicationStatus { get; set; }

    public DateTime? LastPublishedAt { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    public DateTime UpdatedAt { get; set; }
}
