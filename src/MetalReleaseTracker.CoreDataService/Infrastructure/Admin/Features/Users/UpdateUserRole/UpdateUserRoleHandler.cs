using Microsoft.AspNetCore.Identity;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.UpdateUserRole;

public class UpdateUserRoleHandler
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UpdateUserRoleHandler(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<UpdateUserRoleResult> HandleAsync(
        string id,
        UpdateUserRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return new UpdateUserRoleResult { Found = false };
        }

        var roleExists = await _roleManager.RoleExistsAsync(request.Role);
        if (!roleExists)
        {
            await _roleManager.CreateAsync(new IdentityRole(request.Role));
        }

        if (request.Assign)
        {
            await _userManager.AddToRoleAsync(user, request.Role);
        }
        else
        {
            await _userManager.RemoveFromRoleAsync(user, request.Role);
        }

        return new UpdateUserRoleResult { Found = true };
    }
}
