using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("UserFollowedBands")]
public class UserFollowedBandEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public Guid BandId { get; set; }

    [ForeignKey("BandId")]
    public BandEntity Band { get; set; }

    public DateTime CreatedDate { get; set; }
}
