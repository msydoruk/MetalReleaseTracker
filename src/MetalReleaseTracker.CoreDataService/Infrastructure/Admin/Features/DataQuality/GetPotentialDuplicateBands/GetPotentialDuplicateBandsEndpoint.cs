using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetPotentialDuplicateBands;

public static class GetPotentialDuplicateBandsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.DataQuality.PotentialDuplicateBands, async (
                int page,
                int pageSize,
                GetPotentialDuplicateBandsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(page, pageSize, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetPotentialDuplicateBands")
            .WithTags("Admin Data Quality")
            .Produces<DataQualityPagedResult<DuplicateBandGroupDto>>();
    }
}
