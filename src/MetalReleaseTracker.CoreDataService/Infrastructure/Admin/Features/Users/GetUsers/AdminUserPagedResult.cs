namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.GetUsers;

public class AdminUserPagedResult
{
    public List<AdminUserDto> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
