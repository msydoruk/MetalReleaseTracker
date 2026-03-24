using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Events;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class NotificationService : INotificationService
{
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IUserAlbumWatchRepository _userAlbumWatchRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ITelegramBotService _telegramBotService;
    private readonly IAdminSettingsService _adminSettingsService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUserNotificationRepository userNotificationRepository,
        IUserAlbumWatchRepository userAlbumWatchRepository,
        IFileStorageService fileStorageService,
        ITelegramBotService telegramBotService,
        IAdminSettingsService adminSettingsService,
        ILogger<NotificationService> logger)
    {
        _userNotificationRepository = userNotificationRepository;
        _userAlbumWatchRepository = userAlbumWatchRepository;
        _fileStorageService = fileStorageService;
        _telegramBotService = telegramBotService;
        _adminSettingsService = adminSettingsService;
        _logger = logger;
    }

    public async Task GenerateNotificationsAsync(
        AlbumProcessedPublicationEvent albumEvent,
        AlbumEntity? existingAlbum,
        Guid bandId,
        CancellationToken cancellationToken = default)
    {
        var notificationsEnabled = await _adminSettingsService.GetBoolSettingAsync(
            SettingCategories.FeatureToggles,
            SettingKeys.FeatureToggles.NotificationsEnabled,
            true,
            cancellationToken);

        if (!notificationsEnabled)
        {
            return;
        }

        var canonicalTitle = existingAlbum?.CanonicalTitle ?? albumEvent.CanonicalTitle ?? albumEvent.Name;
        var watcherUserIds = await _userAlbumWatchRepository.GetWatcherUserIdsAsync(bandId, canonicalTitle, cancellationToken);

        if (watcherUserIds.Count == 0)
        {
            return;
        }

        var notifications = new List<UserNotificationEntity>();
        var albumName = albumEvent.Name;
        var bandName = albumEvent.BandName;

        if (existingAlbum != null && albumEvent.Price < existingAlbum.Price && existingAlbum.Price > 0)
        {
            var title = $"Price drop: {bandName} - {albumName}";
            var message = $"Price dropped from €{existingAlbum.Price:F2} to €{albumEvent.Price:F2}";
            notifications.AddRange(CreateNotificationsForUsers(watcherUserIds, existingAlbum.Id, NotificationType.PriceDrop, title, message));
        }

        if (existingAlbum?.StockStatus == AlbumStockStatus.OutOfStock && albumEvent.StockStatus == AlbumStockStatus.InStock)
        {
            var title = $"Back in stock: {bandName} - {albumName}";
            var message = $"{albumName} is now available for purchase";
            notifications.AddRange(CreateNotificationsForUsers(watcherUserIds, existingAlbum.Id, NotificationType.BackInStock, title, message));
        }
        else if (existingAlbum?.StockStatus == AlbumStockStatus.OutOfStock
            && albumEvent.StockStatus != AlbumStockStatus.OutOfStock
            && albumEvent.StockStatus != null)
        {
            var title = $"Restock: {bandName} - {albumName}";
            var message = $"{albumName} status changed to {albumEvent.StockStatus}";
            notifications.AddRange(CreateNotificationsForUsers(watcherUserIds, existingAlbum.Id, NotificationType.Restock, title, message));
        }

        if (existingAlbum == null)
        {
            var title = $"New variant: {bandName} - {albumName}";
            var message = $"A new pressing of {albumName} is now available";
            notifications.AddRange(CreateNotificationsForUsers(watcherUserIds, Guid.Empty, NotificationType.NewVariant, title, message));
        }

        if (notifications.Count > 0)
        {
            await _userNotificationRepository.AddBatchAsync(notifications, cancellationToken);

            try
            {
                await _telegramBotService.SendNotificationsAsync(notifications, cancellationToken);
            }
            catch (Exception telegramException)
            {
                _logger.LogWarning(telegramException, "Failed to send Telegram notifications for album {AlbumName}", albumName);
            }

            _logger.LogInformation(
                "Generated {NotificationCount} notifications for album {AlbumName} by {BandName}",
                notifications.Count,
                albumName,
                bandName);
        }
    }

    public async Task<PagedResultDto<NotificationDto>> GetNotificationsAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var result = await _userNotificationRepository.GetPagedAsync(userId, page, pageSize, cancellationToken);

        var dtos = new List<NotificationDto>();
        foreach (var notification in result.Items)
        {
            var dto = new NotificationDto
            {
                Id = notification.Id,
                AlbumId = notification.AlbumId,
                AlbumName = notification.Album?.Name ?? string.Empty,
                BandName = notification.Album?.Band?.Name ?? string.Empty,
                AlbumSlug = notification.Album?.Slug,
                NotificationType = notification.NotificationType.ToString(),
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedDate = notification.CreatedDate
            };

            if (!string.IsNullOrEmpty(notification.Album?.PhotoUrl))
            {
                dto.PhotoUrl = await _fileStorageService.GetFileUrlAsync(notification.Album.PhotoUrl, cancellationToken);
            }

            dtos.Add(dto);
        }

        return new PagedResultDto<NotificationDto>
        {
            Items = dtos,
            TotalCount = result.TotalCount,
            PageCount = result.PageCount,
            PageSize = result.PageSize,
            CurrentPage = result.CurrentPage
        };
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _userNotificationRepository.GetUnreadCountAsync(userId, cancellationToken);
    }

    public async Task MarkAsReadAsync(string userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        await _userNotificationRepository.MarkAsReadAsync(userId, notificationId, cancellationToken);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        await _userNotificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
    }

    private static List<UserNotificationEntity> CreateNotificationsForUsers(
        List<string> userIds,
        Guid albumId,
        NotificationType notificationType,
        string title,
        string message)
    {
        return userIds.Select(userId => new UserNotificationEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AlbumId = albumId,
            NotificationType = notificationType,
            Title = title,
            Message = message,
            IsRead = false,
            CreatedDate = DateTime.UtcNow
        }).ToList();
    }
}
