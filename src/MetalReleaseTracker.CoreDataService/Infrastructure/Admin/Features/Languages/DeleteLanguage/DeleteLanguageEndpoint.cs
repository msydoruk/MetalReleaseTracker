using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.DeleteLanguage;

public static class DeleteLanguageEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete(AdminRouteConstants.Languages.Delete, async (
                string code,
                DeleteLanguageHandler handler,
                CancellationToken cancellationToken) =>
            {
                var error = await handler.HandleAsync(code, cancellationToken);

                if (error == "not_found")
                {
                    return Results.NotFound();
                }

                if (error is not null)
                {
                    return Results.BadRequest(new { message = error });
                }

                return Results.NoContent();
            })
            .WithName("AdminDeleteLanguage")
            .WithTags("Admin Languages")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
