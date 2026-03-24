using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Telegram.GetTelegramStats;

public class GetTelegramStatsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetTelegramStatsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<TelegramStatsResponse> HandleAsync(CancellationToken cancellationToken = default)
    {
        var totalLinksCount = await _context.TelegramLinks
            .AsNoTracking()
            .CountAsync(cancellationToken);

        var linkedUsersCount = await _context.TelegramLinks
            .AsNoTracking()
            .Select(link => link.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new TelegramStatsResponse
        {
            LinkedUsersCount = linkedUsersCount,
            TotalLinksCount = totalLinksCount,
        };
    }
}
