using System.ComponentModel.DataAnnotations;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class SubmitRatingRequest
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
}
