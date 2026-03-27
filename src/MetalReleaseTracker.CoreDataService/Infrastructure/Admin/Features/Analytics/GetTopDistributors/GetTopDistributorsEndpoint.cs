using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Analytics.GetTopDistributors;

public static class GetTopDistributorsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Analytics.TopDistributors, async (
                [AsParameters] AnalyticsDateRangeFilter filter,
                GetTopDistributorsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetTopDistributors")
            .WithTags("Admin Analytics")
            .Produces<List<DistributorAlbumCountDto>>();
    }
}
