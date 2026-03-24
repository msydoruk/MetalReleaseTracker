using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbumById;

public static class GetAlbumByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Albums.GetById, async (
                Guid id,
                GetAlbumByIdHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, cancellationToken);
                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound();
            })
            .WithName("AdminGetAlbumById")
            .WithTags("Admin Albums")
            .Produces<AdminAlbumDetailDto>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
