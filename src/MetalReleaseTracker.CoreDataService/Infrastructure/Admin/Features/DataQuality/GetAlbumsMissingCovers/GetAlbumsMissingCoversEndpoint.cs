using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetAlbumsMissingCovers;

public static class GetAlbumsMissingCoversEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.DataQuality.AlbumsMissingCovers, async (
                int page,
                int pageSize,
                GetAlbumsMissingCoversHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(page, pageSize, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetAlbumsMissingCovers")
            .WithTags("Admin Data Quality")
            .Produces<DataQualityPagedResult<DataQualityAlbumDto>>();
    }
}
