using System.ComponentModel.DataAnnotations;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Review;

public class SubmitReviewRequest
{
    [Required]
    public string Message { get; set; }
}
