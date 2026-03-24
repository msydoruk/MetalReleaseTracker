namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Reviews.GetReviews;

public class AdminReviewDto
{
    public Guid Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
}
