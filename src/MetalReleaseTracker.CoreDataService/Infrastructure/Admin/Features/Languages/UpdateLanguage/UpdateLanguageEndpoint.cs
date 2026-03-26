using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.GetLanguages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.UpdateLanguage;

public static class UpdateLanguageEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.Languages.Update, async (
                string code,
                UpdateLanguageRequest request,
                UpdateLanguageHandler handler,
                CancellationToken cancellationToken) =>
            {
                var (result, error) = await handler.HandleAsync(code, request, cancellationToken);

                if (error == "not_found")
                {
                    return Results.NotFound();
                }

                if (error is not null)
                {
                    return Results.BadRequest(new { message = error });
                }

                return Results.Ok(result);
            })
            .WithName("AdminUpdateLanguage")
            .WithTags("Admin Languages")
            .Produces<LanguageDto>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
