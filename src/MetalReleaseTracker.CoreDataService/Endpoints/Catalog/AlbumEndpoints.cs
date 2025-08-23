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
    }
}