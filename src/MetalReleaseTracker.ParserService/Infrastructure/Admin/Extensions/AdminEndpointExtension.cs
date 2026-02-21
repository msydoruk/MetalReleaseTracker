using MetalReleaseTracker.ParserService.Infrastructure.Admin.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Extensions;

public static class AdminEndpointExtension
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        AdminAuthEndpoints.MapEndpoints(app);

        var adminGroup = app.MapGroup(string.Empty).RequireAuthorization().DisableAntiforgery();
        BandReferenceEndpoints.MapEndpoints(adminGroup);
        CatalogueIndexEndpoints.MapEndpoints(adminGroup);
        ParsingSessionEndpoints.MapEndpoints(adminGroup);
        AiVerificationEndpoints.MapEndpoints(adminGroup);
    }
}
