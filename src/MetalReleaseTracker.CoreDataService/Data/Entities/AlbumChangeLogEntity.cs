using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("AlbumChangeLogs")]
public class AlbumChangeLogEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string AlbumName { get; set; }

    [Required]
    [MaxLength(500)]
    public string BandName { get; set; }

    [Required]
    [MaxLength(200)]
    public string DistributorName { get; set; }

    public float Price { get; set; }

    public float? OldPrice { get; set; }

    public string? PurchaseUrl { get; set; }

    [MaxLength(250)]
    public string? AlbumSlug { get; set; }

    [Required]
    [MaxLength(50)]
    public string ChangeType { get; set; }

    [Required]
    public DateTime ChangedAt { get; set; }
}
