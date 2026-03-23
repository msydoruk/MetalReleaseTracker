using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MetalReleaseTracker.CoreDataService.Middleware;

public class SlugRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public SlugRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value;

        if (string.IsNullOrEmpty(path) || path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        if (TryExtractGuidFromPath(path, "/albums/", out var albumGuid))
        {
            var albumRepository = context.RequestServices.GetRequiredService<IAlbumRepository>();
            var album = await albumRepository.GetAsync(albumGuid, context.RequestAborted);
            if (album != null && !string.IsNullOrEmpty(album.Slug))
            {
                context.Response.StatusCode = 301;
                context.Response.Headers.Location = $"/albums/{album.Slug}";
                return;
            }
        }
        else if (TryExtractGuidFromPath(path, "/bands/", out var bandGuid))
        {
            var bandRepository = context.RequestServices.GetRequiredService<IBandRepository>();
            var band = await bandRepository.GetByIdAsync(bandGuid, context.RequestAborted);
            if (band != null && !string.IsNullOrEmpty(band.Slug))
            {
                context.Response.StatusCode = 301;
                context.Response.Headers.Location = $"/bands/{band.Slug}";
                return;
            }
        }

        await _next(context);
    }

    private static bool TryExtractGuidFromPath(string path, string prefix, out Guid guid)
    {
        guid = Guid.Empty;

        if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var segment = path[prefix.Length..].TrimEnd('/');
        return Guid.TryParse(segment, out guid);
    }
}
