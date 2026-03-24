using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;

public static class GetNavigationItemsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Navigation.GetAll, async (
                GetNavigationItemsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetNavigationItems")
            .WithTags("Admin Navigation")
            .Produces<List<NavigationItemDto>>();
    }
}
