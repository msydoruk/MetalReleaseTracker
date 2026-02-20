using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MetalReleaseTracker.CoreDataService.Data.Entities;

[Table("Feedbacks")]
public class FeedbackEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Message { get; set; }

    public string? Email { get; set; }

    public DateTime CreatedDate { get; set; }
}
