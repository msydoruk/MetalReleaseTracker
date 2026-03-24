using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Notifications.GetNotificationStats;

public static class GetNotificationStatsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Notifications.Stats, async (
                GetNotificationStatsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetNotificationStats")
            .WithTags("Admin Notifications")
            .Produces<NotificationStatsResponse>();
    }
}
