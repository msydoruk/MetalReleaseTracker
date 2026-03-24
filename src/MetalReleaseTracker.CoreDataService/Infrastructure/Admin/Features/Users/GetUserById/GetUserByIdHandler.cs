using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.GetUserById;

public class GetUserByIdHandler
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly CoreDataServiceDbContext _context;

    public GetUserByIdHandler(
        UserManager<IdentityUser> userManager,
        CoreDataServiceDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<AdminUserDetailDto?> HandleAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        var favoritesCount = await _context.UserFavorites
            .AsNoTracking()
            .CountAsync(
                favorite => favorite.UserId == id,
                cancellationToken);

        var followedBandsCount = await _context.UserFollowedBands
            .AsNoTracking()
            .CountAsync(
                follow => follow.UserId == id,
                cancellationToken);

        return new AdminUserDetailDto
        {
            Id = user.Id,
            Email = user.Email,
            UserName = user.UserName,
            EmailConfirmed = user.EmailConfirmed,
            LockoutEnd = user.LockoutEnd,
            LockoutEnabled = user.LockoutEnabled,
            Roles = roles.ToList(),
            FavoritesCount = favoritesCount,
            FollowedBandsCount = followedBandsCount,
        };
    }
}
