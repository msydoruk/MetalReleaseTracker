using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class UserAlbumWatchEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(RouteConstants.Api.Watches.Watch, async (
                Guid albumId,
                IUserAlbumWatchService userAlbumWatchService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await userAlbumWatchService.WatchAlbumAsync(userId, albumId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("WatchAlbum")
            .WithTags("Watches")
            .Produces(200)
            .Produces(401);

        endpoints.MapDelete(RouteConstants.Api.Watches.Unwatch, async (
                Guid albumId,
                IUserAlbumWatchService userAlbumWatchService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await userAlbumWatchService.UnwatchAlbumAsync(userId, albumId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("UnwatchAlbum")
            .WithTags("Watches")
            .Produces(200)
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Watches.Check, async (
                Guid albumId,
                IUserAlbumWatchService userAlbumWatchService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var isWatching = await userAlbumWatchService.IsWatchingAsync(userId, albumId, cancellationToken);
                return Results.Ok(isWatching);
            })
            .RequireAuthorization()
            .WithName("CheckWatching")
            .WithTags("Watches")
            .Produces<bool>()
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Watches.GetKeys, async (
                IUserAlbumWatchService userAlbumWatchService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var keys = await userAlbumWatchService.GetWatchedKeysAsync(userId, cancellationToken);
                return Results.Ok(keys);
            })
            .RequireAuthorization()
            .WithName("GetWatchedKeys")
            .WithTags("Watches")
            .Produces<Dictionary<string, bool>>()
            .Produces(401);
    }
}
