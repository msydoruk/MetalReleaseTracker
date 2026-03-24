using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Notifications.SendBroadcast;

public class SendBroadcastHandler
{
    private readonly CoreDataServiceDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public SendBroadcastHandler(
        CoreDataServiceDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
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
            AlbumId = Guid.Empty,
            NotificationType = notificationType,
            Title = "Broadcast",
            Message = request.Content,
            IsRead = false,
            CreatedDate = DateTime.UtcNow,
        }).ToList();

        _context.UserNotifications.AddRange(notifications);
        await _context.SaveChangesAsync(cancellationToken);

        return new SendBroadcastResponse
        {
            CreatedCount = notifications.Count,
        };
    }
}
