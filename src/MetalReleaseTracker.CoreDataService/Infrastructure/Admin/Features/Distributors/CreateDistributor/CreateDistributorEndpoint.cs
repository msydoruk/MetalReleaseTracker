using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.CreateDistributor;

public static class CreateDistributorEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.Distributors.Create, async (
                CreateDistributorRequest request,
                CreateDistributorHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result is not null
                    ? Results.Created($"{AdminRouteConstants.Distributors.GetAll}/{result.Id}", result)
                    : Results.BadRequest("Invalid distributor code.");
            })
            .WithName("CreateDistributor")
            .WithTags("Admin Distributors")
            .Produces<AdminDistributorDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
