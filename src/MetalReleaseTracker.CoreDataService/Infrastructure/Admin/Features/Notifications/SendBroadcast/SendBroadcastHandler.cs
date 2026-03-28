using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Notifications.SendBroadcast;

public class SendBroadcastHandler
{
    private readonly CoreDataServiceDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITelegramBotService _telegramBotService;
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly IDiscordNotificationService _discordNotificationService;
    private readonly ILogger<SendBroadcastHandler> _logger;

    public SendBroadcastHandler(
        CoreDataServiceDbContext context,
        UserManager<IdentityUser> userManager,
        ITelegramBotService telegramBotService,
        IEmailNotificationService emailNotificationService,
        IDiscordNotificationService discordNotificationService,
        ILogger<SendBroadcastHandler> logger)
    {
        _context = context;
        _userManager = userManager;
        _telegramBotService = telegramBotService;
        _emailNotificationService = emailNotificationService;
        _discordNotificationService = discordNotificationService;
        _logger = logger;
    }

    public async Task<SendBroadcastResponse> HandleAsync(
        SendBroadcastRequest request,
        CancellationToken cancellationToken = default)
    {
        var userIds = request.UserIds;

        if (userIds is null || userIds.Count == 0)
        {
            userIds = await _userManager.Users
                .Select(user => user.Id)
                .ToListAsync(cancellationToken);
        }

        if (!Enum.TryParse<NotificationType>(request.NotificationType, ignoreCase: true, out var notificationType))
        {
            notificationType = NotificationType.NewVariant;
        }

        var notifications = userIds.Select(userId => new UserNotificationEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AlbumId = null,
            NotificationType = notificationType,
            Title = "Broadcast",
            Message = request.Content,
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
        }).ToList();

        _context.UserNotifications.AddRange(notifications);
        await _context.SaveChangesAsync(cancellationToken);

        var channel = request.Channel?.ToLowerInvariant() ?? "all";

        var telegramSentCount = 0;
        if (channel is "all" or "telegram")
        {
            try
            {
                telegramSentCount = await _telegramBotService.SendNotificationsAsync(notifications, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to send broadcast Telegram notifications");
            }
        }

        var emailSentCount = 0;
        if (channel is "all" or "email")
        {
            try
            {
                emailSentCount = await _emailNotificationService.SendNotificationsAsync(notifications, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to send broadcast email notifications");
            }
        }

        var discordSentCount = 0;
        if (channel is "all" or "discord")
        {
            try
            {
                discordSentCount = await _discordNotificationService.SendNotificationsAsync(notifications, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to send broadcast Discord notifications");
            }
        }

        return new SendBroadcastResponse
        {
            CreatedCount = notifications.Count,
            TelegramSentCount = telegramSentCount,
            EmailSentCount = emailSentCount,
            DiscordSentCount = discordSentCount,
        };
    }
}
