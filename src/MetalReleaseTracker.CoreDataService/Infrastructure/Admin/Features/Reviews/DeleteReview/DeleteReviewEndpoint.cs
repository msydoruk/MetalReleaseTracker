using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Reviews.DeleteReview;

public static class DeleteReviewEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(AdminRouteConstants.Reviews.Delete, async (
                Guid id,
                DeleteReviewHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, cancellationToken);
                return result
                    ? Results.NoContent()
                    : Results.NotFound();
            })
            .WithName("AdminDeleteReview")
            .WithTags("Admin Reviews")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
