namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.GetUsers;

public class AdminUserFilterDto
{
    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
