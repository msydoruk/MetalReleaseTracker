namespace MetalReleaseTracker.CoreDataService.Services.Configuration;

public class TelegramBotSettings
{
    public string BotToken { get; set; } = string.Empty;

    public string WebhookBaseUrl { get; set; } = string.Empty;

    public string BotUsername { get; set; } = string.Empty;
}
