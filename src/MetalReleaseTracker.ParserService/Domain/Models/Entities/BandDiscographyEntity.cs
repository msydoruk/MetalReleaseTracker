using System.ComponentModel.DataAnnotations;

namespace MetalReleaseTracker.ParserService.Domain.Models.Entities;

public class BandDiscographyEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid BandReferenceId { get; set; }

    [Required]
    public string AlbumTitle { get; set; }

    [Required]
    public string NormalizedAlbumTitle { get; set; }

    [Required]
    public string AlbumType { get; set; }

    public int? Year { get; set; }

    public BandReferenceEntity BandReference { get; set; }
}
