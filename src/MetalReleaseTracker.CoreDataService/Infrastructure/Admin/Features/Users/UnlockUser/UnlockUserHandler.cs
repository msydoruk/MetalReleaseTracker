using Microsoft.AspNetCore.Identity;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.UnlockUser;

public class UnlockUserHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public UnlockUserHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return false;
        }

        await _userManager.SetLockoutEndDateAsync(user, null);

        return true;
    }
}
