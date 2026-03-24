namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.LockUser;

public class LockUserRequest
{
    public DateTime? LockoutEndUtc { get; set; }
}
