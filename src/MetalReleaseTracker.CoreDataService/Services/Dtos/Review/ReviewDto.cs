namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Review;

public class ReviewDto
{
    public Guid Id { get; set; }

    public string UserName { get; set; }

    public string Message { get; set; }

    public DateTime CreatedDate { get; set; }
}
