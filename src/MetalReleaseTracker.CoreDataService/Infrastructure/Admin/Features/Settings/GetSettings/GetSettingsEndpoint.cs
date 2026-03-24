using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Settings.GetSettings;

public static class GetSettingsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(AdminRouteConstants.Settings.GetByCategory, async (
                string category,
                IAdminSettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.GetSettingsByCategoryAsync(category, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("GetSettingsByCategory")
            .WithTags("Admin Settings")
            .Produces<CategorySettingsDto>();
    }
}
