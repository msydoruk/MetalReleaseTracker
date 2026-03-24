namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.GetUsers;

public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? UserName { get; set; }

    public bool EmailConfirmed { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public List<string> Roles { get; set; } = [];
}
