using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.UpdateBand;

public static class UpdateBandEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.Bands.Update, async (
                Guid id,
                UpdateBandRequest request,
                UpdateBandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, request, cancellationToken);
                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound();
            })
            .WithName("AdminUpdateBand")
            .WithTags("Admin Bands")
            .Produces<AdminBandDto>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
