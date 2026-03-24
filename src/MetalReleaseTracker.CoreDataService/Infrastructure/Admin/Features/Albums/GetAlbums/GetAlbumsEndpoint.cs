using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;

public static class GetAlbumsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Albums.GetAll, async (
                [AsParameters] AdminAlbumFilterDto filter,
                GetAlbumsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetAlbums")
            .WithTags("Admin Albums")
            .Produces<AdminAlbumPagedResult>();
    }
}
