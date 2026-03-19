using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Review;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class ReviewEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Api.Reviews.GetAll, async (
                IReviewService reviewService,
                CancellationToken cancellationToken) =>
            {
                var reviews = await reviewService.GetAllAsync(cancellationToken);
                return Results.Ok(reviews);
            })
            .WithName("GetReviews")
            .WithTags("Reviews")
            .Produces<List<ReviewDto>>()
            .Produces(400);

        endpoints.MapPost(RouteConstants.Api.Reviews.Submit, async (
                SubmitReviewRequest request,
                IReviewService reviewService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userName = user.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(userName))
                {
                    return Results.Unauthorized();
                }

                await reviewService.SubmitAsync(userName, request, cancellationToken);
                return Results.Created();
            })
            .RequireAuthorization()
            .WithName("SubmitReview")
            .WithTags("Reviews")
            .Produces(201)
            .Produces(400)
            .Produces(401);
    }
}
