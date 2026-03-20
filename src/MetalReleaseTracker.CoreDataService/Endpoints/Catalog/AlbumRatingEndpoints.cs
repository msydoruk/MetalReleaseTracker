using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class AlbumRatingEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(RouteConstants.Api.Ratings.Submit, async (
                Guid albumId,
                SubmitRatingRequest request,
                IAlbumRatingService albumRatingService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await albumRatingService.SubmitRatingAsync(userId, albumId, request.Rating, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("SubmitRating")
            .WithTags("Ratings")
            .Produces(200)
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Ratings.Get, async (
                Guid albumId,
                IAlbumRatingService albumRatingService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await albumRatingService.GetRatingAsync(albumId, userId, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetRating")
            .WithTags("Ratings")
            .Produces<AlbumRatingDto>()
            .Produces(400);

        endpoints.MapDelete(RouteConstants.Api.Ratings.Delete, async (
                Guid albumId,
                IAlbumRatingService albumRatingService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await albumRatingService.DeleteRatingAsync(userId, albumId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("DeleteRating")
            .WithTags("Ratings")
            .Produces(200)
            .Produces(401);
    }
}
