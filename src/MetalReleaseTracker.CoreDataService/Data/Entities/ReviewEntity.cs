using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("Reviews")]
public class ReviewEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    public string Message { get; set; }

    public DateTime CreatedDate { get; set; }
}
