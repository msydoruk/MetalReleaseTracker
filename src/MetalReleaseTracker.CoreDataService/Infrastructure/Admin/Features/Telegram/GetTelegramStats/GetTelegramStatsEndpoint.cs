using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Telegram.GetTelegramStats;

public static class GetTelegramStatsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Telegram.Stats, async (
                GetTelegramStatsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetTelegramStats")
            .WithTags("Admin Telegram")
            .Produces<TelegramStatsResponse>();
    }
}
