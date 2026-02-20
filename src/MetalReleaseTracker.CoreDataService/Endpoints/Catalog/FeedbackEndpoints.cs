using MetalReleaseTracker.CoreDataService.Services.Dtos.Feedback;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class FeedbackEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(RouteConstants.Api.Feedback.Submit, async (
                SubmitFeedbackRequest request,
                IFeedbackService feedbackService,
                CancellationToken cancellationToken) =>
            {
                await feedbackService.SubmitAsync(request, cancellationToken);
                return Results.Created();
            })
            .WithName("SubmitFeedback")
            .WithTags("Feedback")
            .Produces(201)
            .Produces(400);
    }
}
