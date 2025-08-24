using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions;

public static class AppSettingsRegistrationExtension
{
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioFileStorageConfig>(configuration.GetSection("Minio"));
        return services;
    }
}