using System.Security.Cryptography;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class TelegramBotService : ITelegramBotService
{
    private readonly ITelegramBotClient? _botClient;
    private readonly ITelegramLinkRepository _telegramLinkRepository;
    private readonly IUserAlbumWatchRepository _userAlbumWatchRepository;
    private readonly ILogger<TelegramBotService> _logger;

    public TelegramBotService(
        ITelegramLinkRepository telegramLinkRepository,
        IUserAlbumWatchRepository userAlbumWatchRepository,
        ILogger<TelegramBotService> logger,
        ITelegramBotClient? botClient = null)
    {
        _botClient = botClient;
        _telegramLinkRepository = telegramLinkRepository;
        _userAlbumWatchRepository = userAlbumWatchRepository;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken = default)
    {
        if (_botClient == null || update.Message?.Text == null)
        {
            return;
        }

        var chatId = update.Message.Chat.Id;
        var text = update.Message.Text.Trim();

        try
        {
            if (text.StartsWith("/start "))
            {
                await HandleStartCommandAsync(chatId, text[7..].Trim(), cancellationToken);
            }
            else if (text == "/start")
            {
                await _botClient.SendMessage(chatId, "Welcome to Metal Release Tracker bot!\n\nTo link your account, generate a token on the website profile page and send:\n/start {token}", cancellationToken: cancellationToken);
            }
            else if (text == "/my")
            {
                await HandleMyCommandAsync(chatId, cancellationToken);
            }
            else if (text == "/help")
            {
                await HandleHelpCommandAsync(chatId, cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error handling Telegram update for chat {ChatId}", chatId);
        }
    }

    public async Task SendNotificationsAsync(List<UserNotificationEntity> notifications, CancellationToken cancellationToken = default)
    {
        if (_botClient == null || notifications.Count == 0)
        {
            return;
        }

        var userIds = notifications.Select(notification => notification.UserId).Distinct().ToList();
        var chatIds = await _telegramLinkRepository.GetChatIdsByUserIdsAsync(userIds, cancellationToken);

        if (chatIds.Count == 0)
        {
            return;
        }

        foreach (var notification in notifications)
        {
            if (!chatIds.TryGetValue(notification.UserId, out var chatId))
            {
                continue;
            }

            try
            {
                var message = FormatNotificationMessage(notification);
                await _botClient.SendMessage(chatId, message, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Failed to send Telegram notification to chat {ChatId}", chatId);
            }
        }
    }

    public async Task<string> GenerateLinkTokenAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _telegramLinkRepository.CleanExpiredTokensAsync(cancellationToken);

        var token = GenerateRandomToken();
        var entity = new TelegramLinkTokenEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await _telegramLinkRepository.AddTokenAsync(entity, cancellationToken);
        return token;
    }

    public async Task<bool> IsLinkedAsync(string userId, CancellationToken cancellationToken = default)
    {
        var link = await _telegramLinkRepository.GetByUserIdAsync(userId, cancellationToken);
        return link != null;
    }

    public async Task UnlinkAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _telegramLinkRepository.RemoveByUserIdAsync(userId, cancellationToken);
    }

    private async Task HandleStartCommandAsync(long chatId, string token, CancellationToken cancellationToken)
    {
        var tokenEntity = await _telegramLinkRepository.GetTokenAsync(token, cancellationToken);
        if (tokenEntity == null)
        {
            await _botClient.SendMessage(chatId, "Invalid or expired token. Please generate a new one from your profile page.", cancellationToken: cancellationToken);
            return;
        }

        var existingLink = await _telegramLinkRepository.GetByUserIdAsync(tokenEntity.UserId, cancellationToken);
        if (existingLink != null)
        {
            await _telegramLinkRepository.RemoveByUserIdAsync(tokenEntity.UserId, cancellationToken);
        }

        var link = new TelegramLinkEntity
        {
            Id = Guid.NewGuid(),
            UserId = tokenEntity.UserId,
            ChatId = chatId,
            LinkedAt = DateTime.UtcNow
        };

        await _telegramLinkRepository.AddAsync(link, cancellationToken);
        await _telegramLinkRepository.RemoveTokenAsync(tokenEntity.Id, cancellationToken);

        await _botClient.SendMessage(chatId, "Account linked successfully! You will now receive notifications for price drops, restocks, and new releases for your watched albums.", cancellationToken: cancellationToken);

        _logger.LogInformation("Telegram account linked for user {UserId} to chat {ChatId}", tokenEntity.UserId, chatId);
    }

    private async Task HandleMyCommandAsync(long chatId, CancellationToken cancellationToken)
    {
        var link = await _telegramLinkRepository.GetByChatIdAsync(chatId, cancellationToken);
        if (link == null)
        {
            await _botClient.SendMessage(chatId, "Your Telegram account is not linked. Use /start {token} to link it.", cancellationToken: cancellationToken);
            return;
        }

        var watchedKeys = await _userAlbumWatchRepository.GetWatchedKeysAsync(link.UserId, cancellationToken);
        if (watchedKeys.Count == 0)
        {
            await _botClient.SendMessage(chatId, "You are not watching any albums. Visit the website to start watching albums for price drops and restocks.", cancellationToken: cancellationToken);
            return;
        }

        var lines = watchedKeys.Keys
            .Select(key => key.Contains(':') ? key[(key.IndexOf(':') + 1)..] : key)
            .OrderBy(title => title)
            .Select(title => $"• {title}")
            .ToList();

        var message = $"You are watching {lines.Count} album(s):\n\n{string.Join("\n", lines)}";
        await _botClient.SendMessage(chatId, message, cancellationToken: cancellationToken);
    }

    private async Task HandleHelpCommandAsync(long chatId, CancellationToken cancellationToken)
    {
        var helpText = """
                       *Metal Release Tracker Bot*

                       Commands:
                       /start {token} — Link your account
                       /my — List your watched albums
                       /help — Show this help

                       You'll receive notifications for:
                       • Price drops on watched albums
                       • Back-in-stock alerts
                       • New pressings of watched albums

                       Manage your watches at metal-release.com
                       """;

        await _botClient.SendMessage(chatId, helpText, parseMode: ParseMode.Markdown, cancellationToken: cancellationToken);
    }

    private static string FormatNotificationMessage(UserNotificationEntity notification)
    {
        var emoji = notification.NotificationType switch
        {
            Data.Entities.Enums.NotificationType.PriceDrop => "🔻",
            Data.Entities.Enums.NotificationType.BackInStock => "✅",
            Data.Entities.Enums.NotificationType.Restock => "🔄",
            Data.Entities.Enums.NotificationType.NewVariant => "🆕",
            _ => "🔔"
        };

        return $"{emoji} *{notification.Title}*\n{notification.Message}";
    }

    private static string GenerateRandomToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(6);
        return Convert.ToBase64String(bytes)
            .Replace("+", string.Empty)
            .Replace("/", string.Empty)
            .Replace("=", string.Empty)[..8]
            .ToUpperInvariant();
    }
}
