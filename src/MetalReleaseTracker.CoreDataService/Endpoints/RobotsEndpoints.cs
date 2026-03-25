using System.Text;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Constants;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints;

public static class RobotsEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Seo.Robots, async (
                IAdminSettingsService settingsService,
                CancellationToken cancellationToken) =>
            {
                var robotsTxt = await settingsService.GetStringSettingAsync(
                    SettingCategories.Seo,
                    SettingKeys.Seo.RobotsTxt,
                    "User-agent: *\nAllow: /",
                    cancellationToken);

                var siteUrl = await settingsService.GetStringSettingAsync(
                    SettingCategories.Seo,
                    SettingKeys.Seo.SiteUrl,
                    "https://metal-release.com",
                    cancellationToken);

                var content = $"{robotsTxt}\n\nSitemap: {siteUrl}/sitemap.xml";

                return Results.Content(content, "text/plain", Encoding.UTF8);
            })
            .WithName("GetRobotsTxt")
            .WithTags("SEO")
            .ExcludeFromDescription();
    }
}
