using MetalReleaseTracker.CoreDataService.Data.Entities;
using Telegram.Bot.Types;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface ITelegramBotService
{
    Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default);

    Task SendNotificationsAsync(List<UserNotificationEntity> notifications, CancellationToken cancellationToken = default);

    Task<string> GenerateLinkTokenAsync(string userId, CancellationToken cancellationToken = default);

    Task<bool> IsLinkedAsync(string userId, CancellationToken cancellationToken = default);

    Task UnlinkAsync(string userId, CancellationToken cancellationToken = default);
}
