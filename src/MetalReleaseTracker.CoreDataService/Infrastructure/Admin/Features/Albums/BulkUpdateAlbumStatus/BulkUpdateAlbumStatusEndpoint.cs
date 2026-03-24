using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.BulkUpdateAlbumStatus;

public static class BulkUpdateAlbumStatusEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.Albums.BulkStatus, async (
                BulkUpdateAlbumStatusRequest request,
                BulkUpdateAlbumStatusHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result.InvalidStatus
                    ? Results.BadRequest("Invalid stock status value.")
                    : Results.Ok(new { result.UpdatedCount });
            })
            .WithName("AdminBulkUpdateAlbumStatus")
            .WithTags("Admin Albums")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
