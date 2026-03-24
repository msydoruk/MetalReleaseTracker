using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Users.GetUsers;

public static class GetUsersEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Users.GetAll, async (
                [AsParameters] AdminUserFilterDto filter,
                GetUsersHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(filter, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("AdminGetUsers")
            .WithTags("Admin Users")
            .Produces<AdminUserPagedResult>();
    }
}
