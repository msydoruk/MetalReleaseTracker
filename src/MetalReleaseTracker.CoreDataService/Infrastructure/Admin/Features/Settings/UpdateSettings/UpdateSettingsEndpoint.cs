using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Endpoints;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Settings.UpdateSettings;

public static class UpdateSettingsEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(AdminRouteConstants.Settings.UpdateByCategory, async (
                string category,
                CategorySettingsDto request,
                IAdminSettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var result = await settingsService.UpdateSettingsByCategoryAsync(category, request, cancellationToken);
                return Results.Ok(result);
            })
            .WithName("UpdateSettingsByCategory")
            .WithTags("Admin Settings")
            .Produces<CategorySettingsDto>();
    }
}
