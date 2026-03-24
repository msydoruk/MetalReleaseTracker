using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Telegram.GetLinkedUsers;

public class GetLinkedUsersHandler
{
    private readonly CoreDataServiceDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public GetLinkedUsersHandler(
        CoreDataServiceDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<List<TelegramLinkedUserDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var links = await _context.TelegramLinks
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new List<TelegramLinkedUserDto>();

        foreach (var link in links)
        {
            var user = await _userManager.FindByIdAsync(link.UserId);
            result.Add(new TelegramLinkedUserDto
            {
                UserId = link.UserId,
                ChatId = link.ChatId,
                Email = user?.Email,
                UserName = user?.UserName,
            });
        }

        return result;
    }
}
