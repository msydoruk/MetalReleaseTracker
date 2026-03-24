using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBandById;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.MergeBands;

public static class MergeBandsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.Bands.Merge, async (
                MergeBandsRequest request,
                MergeBandsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result.NotFound
                    ? Results.NotFound()
                    : Results.Ok(result.Detail);
            })
            .WithName("MergeBands")
            .WithTags("Admin Bands")
            .Produces<AdminBandDetailDto>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
