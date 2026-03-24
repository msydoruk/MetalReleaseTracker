namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Telegram.GetLinkedUsers;

public class TelegramLinkedUserDto
{
    public string UserId { get; set; } = string.Empty;

    public long ChatId { get; set; }

    public string? Email { get; set; }

    public string? UserName { get; set; }
}
