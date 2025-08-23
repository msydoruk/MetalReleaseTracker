using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class BandEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Api.Bands.GetAll, async (
                IBandService bandService,
                CancellationToken cancellationToken) =>
            {
                var bands = await bandService.GetAllBandsAsync(cancellationToken);
                return Results.Ok(bands);
            })
            .WithName("GetAllBands")
            .WithTags("Bands")
            .Produces<List<BandDto>>();

        endpoints.MapGet(RouteConstants.Api.Bands.GetById, async (
                Guid id,
                IBandService bandService,
                CancellationToken cancellationToken) =>
            {
                var band = await bandService.GetBandByIdAsync(id, cancellationToken);
                return band is null ? Results.NotFound() : Results.Ok(band);
            })
            .WithName("GetBandById")
            .WithTags("Bands")
            .Produces<BandDto>(200)
            .Produces(404);

        endpoints.MapGet(RouteConstants.Api.Bands.GetWithAlbumCount, async (
                IBandService bandService,
                CancellationToken cancellationToken) =>
            {
                var bandsWithCount = await bandService.GetBandsWithAlbumCountAsync(cancellationToken);
                return Results.Ok(bandsWithCount);
            })
            .WithName("GetBandsWithAlbumCount")
            .WithTags("Bands")
            .Produces<List<BandWithAlbumCountDto>>();
    }
}