using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;

public static class GetBandsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Bands.GetAll, async (
                [AsParameters] AdminBandFilterDto filter,
                GetBandsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetBands")
            .WithTags("Admin Bands")
            .Produces<AdminBandPagedResult>();
    }
}
