using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("UserAlbumWatches")]
public class UserAlbumWatchEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public Guid AlbumId { get; set; }

    [ForeignKey("AlbumId")]
    public AlbumEntity Album { get; set; } = null!;

    [MaxLength(500)]
    public string? CanonicalTitle { get; set; }

    [Required]
    public Guid BandId { get; set; }

    [ForeignKey("BandId")]
    public BandEntity Band { get; set; } = null!;

    public DateTime CreatedDate { get; set; }
}
