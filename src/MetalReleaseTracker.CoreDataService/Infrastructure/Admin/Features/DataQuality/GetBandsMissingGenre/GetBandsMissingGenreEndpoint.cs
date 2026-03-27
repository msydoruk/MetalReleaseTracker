using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetBandsMissingGenre;

public static class GetBandsMissingGenreEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.DataQuality.BandsMissingGenre, async (
                int page,
                int pageSize,
                GetBandsMissingGenreHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(page, pageSize, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetBandsMissingGenre")
            .WithTags("Admin Data Quality")
            .Produces<DataQualityPagedResult<DataQualityBandDto>>();
    }
}
