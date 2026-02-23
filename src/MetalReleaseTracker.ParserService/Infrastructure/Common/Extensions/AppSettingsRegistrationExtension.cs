using MetalReleaseTracker.ParserService.Infrastructure.Http.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs.Configuration;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.ParserService.Infrastructure.Common.Extensions;

public static class AppSettingsRegistrationExtension
{
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HttpRequestSettings>(configuration.GetSection("HttpRequestSettings"));
        services.Configure<ImageUploadSettings>(configuration.GetSection("ImageUploadSettings"));
        services.Configure<AlbumParsedPublisherJobSettings>(configuration.GetSection("AlbumParsedPublisherJob"));
        services.Configure<MinioFileStorageConfig>(configuration.GetSection("Minio"));

        return services;
    }
}
