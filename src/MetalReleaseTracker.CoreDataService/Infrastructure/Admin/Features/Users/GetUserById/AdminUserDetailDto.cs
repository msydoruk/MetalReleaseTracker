namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.GetUserById;

public class AdminUserDetailDto
{
    public string Id { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? UserName { get; set; }

    public bool EmailConfirmed { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }

    public bool LockoutEnabled { get; set; }

    public List<string> Roles { get; set; } = [];

    public int FavoritesCount { get; set; }

    public int FollowedBandsCount { get; set; }
}
