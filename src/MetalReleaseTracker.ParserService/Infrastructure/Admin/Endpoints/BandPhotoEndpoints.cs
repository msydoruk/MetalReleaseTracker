using MetalReleaseTracker.ParserService.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;

public static class BandPhotoEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.BandPhotos.Sync, (
                IBandPhotoSyncService bandPhotoSyncService,
                CancellationToken cancellationToken) =>
            {
                _ = Task.Run(() => bandPhotoSyncService.SyncBandPhotosAsync(cancellationToken), cancellationToken);
                return Results.Accepted();
            })
            .WithName("SyncBandPhotos")
            .WithTags("Admin Band Photos")
            .Produces(202);
    }
}
