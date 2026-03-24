namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Reviews.GetReviews;

public class AdminReviewPagedResult
{
    public List<AdminReviewDto> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
