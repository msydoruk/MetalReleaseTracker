using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("UserFavorites")]
public class UserFavoriteEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public Guid AlbumId { get; set; }

    [ForeignKey("AlbumId")]
    public AlbumEntity Album { get; set; }

    public DateTime CreatedDate { get; set; }
}
