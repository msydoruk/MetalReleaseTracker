using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Telegram.GetTelegramStats;

public class GetTelegramStatsHandler
{
    private readonly CoreDataServiceDbContext _context;
    private readonly IAdminSettingsService _settingsService;

    public GetTelegramStatsHandler(
        CoreDataServiceDbContext context,
        IAdminSettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
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

        var botActive = await _settingsService.GetBoolSettingAsync(
            SettingCategories.FeatureToggles,
            SettingKeys.FeatureToggles.TelegramBotEnabled,
            false,
            cancellationToken);

        return new TelegramStatsResponse
        {
            LinkedUsersCount = linkedUsersCount,
            TotalLinksCount = totalLinksCount,
            BotActive = botActive,
        };
    }
}
