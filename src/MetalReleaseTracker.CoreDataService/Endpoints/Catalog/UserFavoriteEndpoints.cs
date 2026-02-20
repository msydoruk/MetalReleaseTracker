using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class UserFavoriteEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(RouteConstants.Api.Favorites.Add, async (
                Guid albumId,
                IUserFavoriteService userFavoriteService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await userFavoriteService.AddFavoriteAsync(userId, albumId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("AddFavorite")
            .WithTags("Favorites")
            .Produces(200)
            .Produces(401);

        endpoints.MapDelete(RouteConstants.Api.Favorites.Remove, async (
                Guid albumId,
                IUserFavoriteService userFavoriteService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await userFavoriteService.RemoveFavoriteAsync(userId, albumId, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("RemoveFavorite")
            .WithTags("Favorites")
            .Produces(200)
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Favorites.GetAll, async (
                int page,
                int pageSize,
                IUserFavoriteService userFavoriteService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var result = await userFavoriteService.GetFavoriteAlbumsAsync(userId, page, pageSize, cancellationToken);
                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetFavorites")
            .WithTags("Favorites")
            .Produces<PagedResultDto<AlbumDto>>()
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Favorites.GetIds, async (
                IUserFavoriteService userFavoriteService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var ids = await userFavoriteService.GetFavoriteIdsAsync(userId, cancellationToken);
                return Results.Ok(ids);
            })
            .RequireAuthorization()
            .WithName("GetFavoriteIds")
            .WithTags("Favorites")
            .Produces<List<Guid>>()
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Favorites.Check, async (
                Guid albumId,
                IUserFavoriteService userFavoriteService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var isFavorite = await userFavoriteService.IsFavoriteAsync(userId, albumId, cancellationToken);
                return Results.Ok(isFavorite);
            })
            .RequireAuthorization()
            .WithName("CheckFavorite")
            .WithTags("Favorites")
            .Produces<bool>()
            .Produces(401);
    }
}
