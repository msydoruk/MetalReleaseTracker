using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.CreateNavigationItem;

public static class CreateNavigationItemEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.Navigation.Create, async (
                CreateNavigationItemRequest request,
                CreateNavigationItemHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return Results.Created($"{AdminRouteConstants.Navigation.GetAll}/{result.Id}", result);
            })
            .WithName("CreateNavigationItem")
            .WithTags("Admin Navigation")
            .Produces<NavigationItemDto>(StatusCodes.Status201Created);
    }
}
