using System.ComponentModel.DataAnnotations;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Feedback;

public class SubmitFeedbackRequest
{
    [Required]
    public string Message { get; set; }

    public string? Email { get; set; }
}
