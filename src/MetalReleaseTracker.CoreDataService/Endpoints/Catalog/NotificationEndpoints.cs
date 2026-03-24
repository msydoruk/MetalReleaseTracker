using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class NotificationEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Api.Notifications.GetAll, async (
                int page,
                int pageSize,
                INotificationService notificationService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var result = await notificationService.GetNotificationsAsync(userId, page, pageSize, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetNotifications")
            .WithTags("Notifications")
            .Produces<PagedResultDto<NotificationDto>>()
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Notifications.UnreadCount, async (
                INotificationService notificationService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var count = await notificationService.GetUnreadCountAsync(userId, cancellationToken);
                return Results.Ok(count);
            })
            .RequireAuthorization()
            .WithName("GetUnreadNotificationCount")
            .WithTags("Notifications")
            .Produces<int>()
            .Produces(401);

        endpoints.MapPut(RouteConstants.Api.Notifications.MarkRead, async (
                Guid notificationId,
                INotificationService notificationService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await notificationService.MarkAsReadAsync(userId, notificationId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("MarkNotificationRead")
            .WithTags("Notifications")
            .Produces(200)
            .Produces(401);

        endpoints.MapPut(RouteConstants.Api.Notifications.MarkAllRead, async (
                INotificationService notificationService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("MarkAllNotificationsRead")
            .WithTags("Notifications")
            .Produces(200)
            .Produces(401);
    }
}
