using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetDataQualitySummary;

public static class GetDataQualitySummaryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.DataQuality.Summary, async (
                GetDataQualitySummaryHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetDataQualitySummary")
            .WithTags("Admin Data Quality")
            .Produces<DataQualitySummaryDto>();
    }
}
