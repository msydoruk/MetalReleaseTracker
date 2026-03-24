namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Notifications.SendBroadcast;

public class SendBroadcastRequest
{
    public string Content { get; set; } = string.Empty;

    public string NotificationType { get; set; } = string.Empty;

    public List<string>? UserIds { get; set; }
}
