using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints.Catalog;

public static class DistributorEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Api.Distributors.GetAll, async (
                IDistributorService distributorService,
                CancellationToken cancellationToken) =>
            {
                var distributors = await distributorService.GetAllDistributorsAsync(cancellationToken);
                return Results.Ok(distributors);
            })
            .WithName("GetAllDistributors")
            .WithTags("Distributors")
            .Produces<List<DistributorDto>>();

        endpoints.MapGet(RouteConstants.Api.Distributors.GetById, async (
                Guid id,
                IDistributorService distributorService,
                CancellationToken cancellationToken) =>
            {
                var distributor = await distributorService.GetDistributorByIdAsync(id, cancellationToken);
                return distributor is null ? Results.NotFound() : Results.Ok(distributor);
            })
            .WithName("GetDistributorById")
            .WithTags("Distributors")
            .Produces<DistributorDto>(200)
            .Produces(404);

        endpoints.MapGet(RouteConstants.Api.Distributors.GetWithAlbumCount, async (
                IDistributorService distributorService,
                CancellationToken cancellationToken) =>
            {
                var distributorsWithCount = await distributorService.GetDistributorsWithAlbumCountAsync(cancellationToken);
                return Results.Ok(distributorsWithCount);
            })
            .WithName("GetDistributorsWithAlbumCount")
            .WithTags("Distributors")
            .Produces<List<DistributorWithAlbumCountDto>>();
    }
}