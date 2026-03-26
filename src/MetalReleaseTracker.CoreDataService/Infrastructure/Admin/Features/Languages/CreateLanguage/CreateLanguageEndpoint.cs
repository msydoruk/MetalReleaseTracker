using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.GetLanguages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.CreateLanguage;

public static class CreateLanguageEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(AdminRouteConstants.Languages.Create, async (
                CreateLanguageRequest request,
                CreateLanguageHandler handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                if (result is null)
                {
                    return Results.BadRequest(new { message = $"Language with code '{request.Code}' already exists." });
                }

                return Results.Created($"{AdminRouteConstants.Languages.GetAll}/{result.Code}", result);
            })
            .WithName("AdminCreateLanguage")
            .WithTags("Admin Languages")
            .Produces<LanguageDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
