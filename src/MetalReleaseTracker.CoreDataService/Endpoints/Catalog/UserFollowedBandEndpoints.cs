using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class UserFollowedBandEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(RouteConstants.Api.FollowedBands.Follow, async (
                Guid bandId,
                IUserFollowedBandService userFollowedBandService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await userFollowedBandService.FollowAsync(userId, bandId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("FollowBand")
            .WithTags("FollowedBands")
            .Produces(200)
            .Produces(401);

        endpoints.MapDelete(RouteConstants.Api.FollowedBands.Unfollow, async (
                Guid bandId,
                IUserFollowedBandService userFollowedBandService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await userFollowedBandService.UnfollowAsync(userId, bandId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("UnfollowBand")
            .WithTags("FollowedBands")
            .Produces(200)
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.FollowedBands.GetAll, async (
                IUserFollowedBandService userFollowedBandService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var bands = await userFollowedBandService.GetFollowedBandsAsync(userId, cancellationToken);
                return Results.Ok(bands);
            })
            .RequireAuthorization()
            .WithName("GetFollowedBands")
            .WithTags("FollowedBands")
            .Produces<List<BandDto>>()
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.FollowedBands.GetIds, async (
                IUserFollowedBandService userFollowedBandService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var ids = await userFollowedBandService.GetFollowedBandIdsAsync(userId, cancellationToken);
                return Results.Ok(ids);
            })
            .RequireAuthorization()
            .WithName("GetFollowedBandIds")
            .WithTags("FollowedBands")
            .Produces<Dictionary<Guid, bool>>()
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.FollowedBands.Check, async (
                Guid bandId,
                IUserFollowedBandService userFollowedBandService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var isFollowing = await userFollowedBandService.IsFollowingAsync(userId, bandId, cancellationToken);
                return Results.Ok(isFollowing);
            })
            .RequireAuthorization()
            .WithName("CheckFollowingBand")
            .WithTags("FollowedBands")
            .Produces<bool>()
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.FollowedBands.Feed, async (
                int page,
                int pageSize,
                IUserFollowedBandService userFollowedBandService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var result = await userFollowedBandService.GetFeedAsync(userId, page, pageSize, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetFollowedBandsFeed")
            .WithTags("FollowedBands")
            .Produces<PagedResultDto<AlbumDto>>()
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.FollowedBands.FollowerCount, async (
                Guid bandId,
                IUserFollowedBandService userFollowedBandService,
                CancellationToken cancellationToken) =>
            {
                var count = await userFollowedBandService.GetFollowerCountAsync(bandId, cancellationToken);
                return Results.Ok(count);
            })
            .WithName("GetBandFollowerCount")
            .WithTags("FollowedBands")
            .Produces<int>();
    }
}
