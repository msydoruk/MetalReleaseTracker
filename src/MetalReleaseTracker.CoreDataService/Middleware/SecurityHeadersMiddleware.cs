using Microsoft.AspNetCore.Http;

namespace MetalReleaseTracker.CoreDataService.Middleware;

public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

        if (context.Request.IsHttps)
        {
            headers["Strict-Transport-Security"] = "max-age=63072000; includeSubDomains; preload";
        }

        await _next(context);
    }
}
