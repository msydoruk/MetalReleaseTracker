namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.UpdateUserRole;

public class UpdateUserRoleRequest
{
    public string Role { get; set; } = string.Empty;

    public bool Assign { get; set; }
}
