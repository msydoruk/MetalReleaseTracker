using Microsoft.AspNetCore.Identity;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.LockUser;

public class LockUserHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public LockUserHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> HandleAsync(
        string id,
        LockUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return false;
        }

        var lockoutEnd = request.LockoutEndUtc.HasValue
            ? new DateTimeOffset(request.LockoutEndUtc.Value, TimeSpan.Zero)
            : DateTimeOffset.MaxValue;

        await _userManager.SetLockoutEnabledAsync(user, true);
        await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);

        return true;
    }
}
