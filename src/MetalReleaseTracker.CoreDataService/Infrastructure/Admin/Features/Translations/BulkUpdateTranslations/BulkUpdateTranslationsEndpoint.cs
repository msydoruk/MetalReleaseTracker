using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.BulkUpdateTranslations;

public static class BulkUpdateTranslationsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.Translations.BulkUpdate, async (
                BulkUpdateTranslationsRequest request,
                BulkUpdateTranslationsHandler handler,
                CancellationToken cancellationToken) =>
            {
                var updatedCount = await handler.HandleAsync(request, cancellationToken);
                return Results.Ok(new { UpdatedCount = updatedCount });
            })
            .WithName("AdminBulkUpdateTranslations")
            .WithTags("Admin Translations")
            .Produces(StatusCodes.Status200OK);
    }
}
