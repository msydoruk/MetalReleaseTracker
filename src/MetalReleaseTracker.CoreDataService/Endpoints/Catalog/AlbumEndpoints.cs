using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class AlbumEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Api.Albums.GetFiltered, async (
                [AsParameters] AlbumFilterDto filter,
                IAlbumService albumService,
                CancellationToken cancellationToken) =>
            {
                var result = await albumService.GetFilteredAlbums(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetFilteredAlbums")
            .WithTags("Albums")
            .Produces<PagedResultDto<AlbumDto>>();

        endpoints.MapGet(RouteConstants.Api.Albums.GetGrouped, async (
                [AsParameters] AlbumFilterDto filter,
                IAlbumService albumService,
                CancellationToken cancellationToken) =>
            {
                var result = await albumService.GetGroupedAlbums(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetGroupedAlbums")
            .WithTags("Albums")
            .Produces<PagedResultDto<GroupedAlbumDto>>();

        endpoints.MapGet(RouteConstants.Api.Albums.GetSuggestions, async (
                string q,
                IAlbumService albumService,
                CancellationToken cancellationToken) =>
            {
                var suggestions = await albumService.GetSuggestions(q, cancellationToken);
                return Results.Ok(suggestions);
            })
            .WithName("GetAlbumSuggestions")
            .WithTags("Albums")
            .Produces<List<AlbumSuggestionDto>>();

        endpoints.MapGet(RouteConstants.Api.Albums.GetById, async (
                Guid id,
                IAlbumService albumService,
                CancellationToken cancellationToken) =>
            {
                var album = await albumService.GetAlbumById(id, cancellationToken);
                return album is null ? Results.NotFound() : Results.Ok(album);
            })
            .WithName("GetAlbumById")
            .WithTags("Albums")
            .Produces<AlbumDto>(200)
            .Produces(404);

        endpoints.MapGet(RouteConstants.Api.Albums.GetDetail, async (
                Guid id,
                IAlbumService albumService,
                CancellationToken cancellationToken) =>
            {
                var detail = await albumService.GetAlbumDetail(id, cancellationToken);
                return detail is null ? Results.NotFound() : Results.Ok(detail);
            })
            .WithName("GetAlbumDetail")
            .WithTags("Albums")
            .Produces<AlbumDetailDto>(200)
            .Produces(404);
    }
}