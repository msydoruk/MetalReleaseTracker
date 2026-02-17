using System.ComponentModel.DataAnnotations;

namespace MetalReleaseTracker.ParserService.Domain.Models.Entities;

public class BandReferenceEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string BandName { get; set; }

    [Required]
    public long MetalArchivesId { get; set; }

    public string Genre { get; set; }

    [Required]
    public DateTime LastSyncedAt { get; set; }

    public List<BandDiscographyEntity> Discography { get; set; } = new();
}
