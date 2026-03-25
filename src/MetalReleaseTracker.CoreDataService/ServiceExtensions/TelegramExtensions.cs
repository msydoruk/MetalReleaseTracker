using MetalReleaseTracker.CoreDataService.Endpoints;
using MetalReleaseTracker.CoreDataService.Services.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Serilog;
using Telegram.Bot;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class TelegramExtensions
{
    public static WebApplication RegisterTelegramWebhook(this WebApplication app)
    {
        var botClient = app.Services.GetService<ITelegramBotClient>();
        if (botClient is null)
        {
            Log.Information("Telegram bot token not configured, skipping webhook registration");
            return app;
        }

        var settings = app.Services.GetRequiredService<IOptions<TelegramBotSettings>>().Value;
        if (string.IsNullOrEmpty(settings.WebhookBaseUrl))
        {
            Log.Warning("Telegram WebhookBaseUrl not configured, skipping webhook registration");
            return app;
        }

        var webhookUrl = $"{settings.WebhookBaseUrl.TrimEnd('/')}{RouteConstants.Api.Telegram.Webhook}";

        try
        {
            botClient.SetWebhook(webhookUrl).GetAwaiter().GetResult();
            Log.Information("Telegram webhook registered: {WebhookUrl}", webhookUrl);
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to register Telegram webhook");
        }

        return app;
    }
}
