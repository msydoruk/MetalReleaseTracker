using MetalReleaseTracker.CoreDataService.Middleware;
using Microsoft.AspNetCore.Builder;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ErrorHandlingMiddleware>();
    }

    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SecurityHeadersMiddleware>();
    }

    public static IApplicationBuilder UseSlugRedirect(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SlugRedirectMiddleware>();
    }
}