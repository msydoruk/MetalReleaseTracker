using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.HideAlbum;

public static class HideAlbumEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.DataQuality.HideAlbum, async (
                Guid id,
                HideAlbumHandler handler,
                CancellationToken cancellationToken) =>
            {
                var success = await handler.HandleAsync(id, cancellationToken);
                return success ? Results.Ok() : Results.NotFound();
            })
            .WithName("AdminHideAlbum")
            .WithTags("Admin Data Quality")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }
}
