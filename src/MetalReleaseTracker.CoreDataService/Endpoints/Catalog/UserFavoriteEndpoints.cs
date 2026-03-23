using System.Security.Claims;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
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
                int? status,
                IUserFavoriteService userFavoriteService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var collectionStatus = status.HasValue ? (UserCollectionStatus)status.Value : UserCollectionStatus.Favorite;
                await userFavoriteService.AddFavoriteAsync(userId, albumId, collectionStatus, cancellationToken);
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

        endpoints.MapPut(RouteConstants.Api.Favorites.UpdateStatus, async (
                Guid albumId,
                UpdateStatusRequest request,
                IUserFavoriteService userFavoriteService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                await userFavoriteService.UpdateStatusAsync(userId, albumId, (UserCollectionStatus)request.Status, cancellationToken);
                return Results.Ok();
            })
            .RequireAuthorization()
            .WithName("UpdateFavoriteStatus")
            .WithTags("Favorites")
            .Produces(200)
            .Produces(401);

        endpoints.MapGet(RouteConstants.Api.Favorites.GetAll, async (
                int page,
                int pageSize,
                int? status,
                IUserFavoriteService userFavoriteService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var collectionStatus = status.HasValue ? (UserCollectionStatus?)status.Value : null;
                var result = await userFavoriteService.GetFavoriteAlbumsAsync(userId, page, pageSize, collectionStatus, cancellationToken);
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
            .Produces<Dictionary<Guid, int>>()
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

        endpoints.MapGet(RouteConstants.Api.Favorites.Export, async (
                string format,
                IUserFavoriteService userFavoriteService,
                ClaimsPrincipal user,
                CancellationToken cancellationToken) =>
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                var bytes = await userFavoriteService.ExportCollectionAsync(userId, format, cancellationToken);
                var fileName = $"collection-{DateTime.UtcNow:yyyy-MM-dd}.csv";
                return Results.File(bytes, "text/csv", fileName);
            })
            .RequireAuthorization()
            .WithName("ExportFavorites")
            .WithTags("Favorites")
            .Produces(200)
            .Produces(401);
    }

    public record UpdateStatusRequest(int Status);
}
