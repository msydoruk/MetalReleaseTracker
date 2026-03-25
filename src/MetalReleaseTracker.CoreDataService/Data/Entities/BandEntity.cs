namespace MetalReleaseTracker.CoreDataService.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Bands")]
public class BandEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "The band name is required.")]
    public string Name { get; set; }

    public string? Description { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Genre { get; set; }

    public string? MetalArchivesUrl { get; set; }

    public int? FormationYear { get; set; }

    [Required]
    [MaxLength(250)]
    public string Slug { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? SeoTitle { get; set; }

    [MaxLength(320)]
    public string? SeoDescription { get; set; }

    [MaxLength(500)]
    public string? SeoKeywords { get; set; }
}
