namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Reviews.GetReviews;

public class AdminReviewFilterDto
{
    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
