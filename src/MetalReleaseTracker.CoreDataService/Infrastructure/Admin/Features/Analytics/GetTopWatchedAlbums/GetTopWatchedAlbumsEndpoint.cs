using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetTopWatchedAlbums;

public static class GetTopWatchedAlbumsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Analytics.TopWatchedAlbums, async (
                GetTopWatchedAlbumsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new AnalyticsDateRangeFilter(),
                    cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetTopWatchedAlbums")
            .WithTags("Admin Analytics")
            .Produces<List<WatchedAlbumDto>>();
    }
}
