using System.Text;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MetalReleaseTracker.CoreDataService.Endpoints;

public static class SitemapEndpoints
{
    public static void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(RouteConstants.Seo.Sitemap, async (
                ISitemapService sitemapService,
                CancellationToken cancellationToken) =>
            {
                var xml = await sitemapService.GenerateSitemapAsync(cancellationToken);
                return Results.Content(xml, "application/xml", Encoding.UTF8);
            })
            .WithName("GetSitemap")
            .WithTags("SEO")
            .ExcludeFromDescription();
    }
}
