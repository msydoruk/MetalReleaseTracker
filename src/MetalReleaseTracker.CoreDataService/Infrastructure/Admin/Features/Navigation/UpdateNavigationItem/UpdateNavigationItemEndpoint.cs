using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.UpdateNavigationItem;

public static class UpdateNavigationItemEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.Navigation.Update, async (
                Guid id,
                UpdateNavigationItemRequest request,
                UpdateNavigationItemHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(id, request, cancellationToken);
                return result is not null
                    ? Results.Ok(result)
                    : Results.NotFound();
            })
            .WithName("UpdateNavigationItem")
            .WithTags("Admin Navigation")
            .Produces<NavigationItemDto>()
            .Produces(StatusCodes.Status404NotFound);
    }
}
