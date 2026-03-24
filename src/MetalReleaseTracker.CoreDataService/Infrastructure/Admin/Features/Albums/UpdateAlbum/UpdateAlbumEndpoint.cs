using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbumById;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.UpdateAlbum;

public static class UpdateAlbumEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.Albums.Update, async (
                Guid id,
                UpdateAlbumRequest request,
                UpdateAlbumHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, request, cancellationToken);
                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound();
            })
            .WithName("AdminUpdateAlbum")
            .WithTags("Admin Albums")
            .Produces<AdminAlbumDetailDto>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
