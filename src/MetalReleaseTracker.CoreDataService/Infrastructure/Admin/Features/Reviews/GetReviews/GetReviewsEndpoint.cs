using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Reviews.GetReviews;

public static class GetReviewsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Reviews.GetAll, async (
                [AsParameters] AdminReviewFilterDto filter,
                GetReviewsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetReviews")
            .WithTags("Admin Reviews")
            .Produces<AdminReviewPagedResult>();
    }
}
