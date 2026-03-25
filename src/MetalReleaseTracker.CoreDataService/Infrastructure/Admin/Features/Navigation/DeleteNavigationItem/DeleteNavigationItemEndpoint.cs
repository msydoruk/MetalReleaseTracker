using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.DeleteNavigationItem;

public static class DeleteNavigationItemEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(AdminRouteConstants.Navigation.Delete, async (
                Guid id,
                DeleteNavigationItemHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, cancellationToken);
                return result
                    ? Results.NoContent()
                    : Results.NotFound();
            })
            .WithName("AdminDeleteNavigationItem")
            .WithTags("Admin Navigation")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }
}
