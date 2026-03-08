using System.Net;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Yarp.ReverseProxy.Forwarder;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class MinioForwarderExtension
{
    public static WebApplication MapMinioForwarder(this WebApplication app)
    {
        var forwarder = app.Services.GetRequiredService<IHttpForwarder>();
        var httpClient = new HttpMessageInvoker(new SocketsHttpHandler
        {
            UseProxy = false,
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            UseCookies = false,
        });

        var minioConfig = app.Services.GetRequiredService<IOptions<MinioFileStorageConfig>>().Value;
        var destinationPrefix = $"http://{minioConfig.Endpoint}/";

        app.Map("/storage/{**catch-all}", async (HttpContext httpContext) =>
        {
            var path = httpContext.Request.Path.Value!;
            var storagePrefixLength = "/storage".Length;
            var remainingPath = path[storagePrefixLength..];

            httpContext.Request.Path = remainingPath;

            var error = await forwarder.SendAsync(
                httpContext,
                destinationPrefix,
                httpClient);

            if (error != ForwarderError.None)
            {
                var errorFeature = httpContext.GetForwarderErrorFeature();
                var exception = errorFeature?.Exception;
                app.Logger.LogError(exception, "MinIO proxy error: {Error}", error);
            }
        });

        return app;
    }
}
