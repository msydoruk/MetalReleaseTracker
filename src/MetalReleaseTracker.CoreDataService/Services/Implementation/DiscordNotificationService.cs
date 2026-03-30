using System.Net.Http.Json;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class DiscordNotificationService : IDiscordNotificationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAdminSettingsService _settingsService;
    private readonly ILogger<DiscordNotificationService> _logger;

    public DiscordNotificationService(
        IHttpClientFactory httpClientFactory,
        IAdminSettingsService settingsService,
        ILogger<DiscordNotificationService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsService = settingsService;
        _logger = logger;
    }

    public async Task<int> SendNotificationsAsync(
        List<UserNotificationEntity> notifications,
        CancellationToken cancellationToken = default)
    {
        var enabled = await _settingsService.GetBoolSettingAsync(
            SettingCategories.Discord,
            SettingKeys.Discord.Enabled,
            false,
            cancellationToken);

        if (!enabled)
        {
            return 0;
        }

        var webhookUrl = await _settingsService.GetStringSettingAsync(
            SettingCategories.Discord,
            SettingKeys.Discord.WebhookUrl,
            string.Empty,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogWarning("Discord webhook URL is not configured");
            return 0;
        }

        var sentCount = 0;
        var uniqueMessages = notifications
            .GroupBy(notification => notification.Message)
            .Select(group => group.First())
            .ToList();

        var client = _httpClientFactory.CreateClient("Discord");

        foreach (var notification in uniqueMessages)
        {
            try
            {
                var embed = new
                {
                    embeds = new[]
                    {
                        new
                        {
                            title = notification.Title,
                            description = notification.Message,
                            color = GetColorForType(notification.NotificationType),
                            timestamp = notification.CreatedDate.ToString("o"),
                        },
                    },
                };

                var response = await client.PostAsJsonAsync(webhookUrl, embed, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    sentCount++;
                }
                else
                {
                    _logger.LogWarning(
                        "Discord webhook returned {StatusCode} for notification {NotificationTitle}",
                        response.StatusCode,
                        notification.Title);
                }

                await Task.Delay(500, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to send Discord notification: {NotificationTitle}", notification.Title);
            }
        }

        _logger.LogInformation("Sent {SentCount} Discord notifications", sentCount);

        return sentCount;
    }

    private static int GetColorForType(NotificationType notificationType)
    {
        return notificationType switch
        {
            NotificationType.PriceDrop => 0x4CAF50,
            NotificationType.PriceIncrease => 0xF44336,
            NotificationType.BackInStock => 0x2196F3,
            NotificationType.Restock => 0xFF9800,
            NotificationType.NewVariant => 0xE53935,
            _ => 0x9E9E9E,
        };
    }
}
