using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetUserGrowth;

public static class GetUserGrowthEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Analytics.UserGrowth, async (
                [AsParameters] AnalyticsDateRangeFilter filter,
                GetUserGrowthHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetUserGrowth")
            .WithTags("Admin Analytics")
            .Produces<List<TimeSeriesDataPoint>>();
    }
}
