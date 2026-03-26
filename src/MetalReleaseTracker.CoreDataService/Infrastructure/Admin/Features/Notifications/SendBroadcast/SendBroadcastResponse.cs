namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Notifications.SendBroadcast;

public class SendBroadcastResponse
{
    public int CreatedCount { get; set; }

    public int TelegramSentCount { get; set; }

    public int EmailSentCount { get; set; }
}
