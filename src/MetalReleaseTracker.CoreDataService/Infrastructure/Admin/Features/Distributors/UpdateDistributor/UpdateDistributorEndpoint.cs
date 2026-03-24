using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.UpdateDistributor;

public static class UpdateDistributorEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.Distributors.Update, async (
                Guid id,
                UpdateDistributorRequest request,
                UpdateDistributorHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, request, cancellationToken);
                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound();
            })
            .WithName("AdminUpdateDistributor")
            .WithTags("Admin Distributors")
            .Produces<AdminDistributorDto>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
