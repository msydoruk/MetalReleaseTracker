using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.GetUsers;

public class GetUsersHandler
{
    private readonly UserManager<IdentityUser> _userManager;

    public GetUsersHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AdminUserPagedResult> HandleAsync(
        AdminUserFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var query = _userManager.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(user =>
                EF.Functions.ILike(user.Email!, $"%{filter.Search}%") ||
                EF.Functions.ILike(user.UserName!, $"%{filter.Search}%"));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .OrderBy(user => user.UserName)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        var items = new List<AdminUserDto>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            items.Add(new AdminUserDto
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd,
                Roles = roles.ToList(),
            });
        }

        return new AdminUserPagedResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = filter.Page,
            PageSize = filter.PageSize,
        };
    }
}
