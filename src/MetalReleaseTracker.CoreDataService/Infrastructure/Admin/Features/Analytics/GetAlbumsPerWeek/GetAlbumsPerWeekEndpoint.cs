using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetAlbumsPerWeek;

public static class GetAlbumsPerWeekEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Analytics.AlbumsPerWeek, async (
                [AsParameters] AnalyticsDateRangeFilter filter,
                GetAlbumsPerWeekHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetAlbumsPerWeek")
            .WithTags("Admin Analytics")
            .Produces<List<TimeSeriesDataPoint>>();
    }
}
