using MetalReleaseTracker.CatalogSyncService.Configurations;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.CatalogSyncService.ServiceExtensions;

public static class AppSettingsRegistrationExtension
{
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AlbumProcessedPublisherJobSettings>(configuration.GetSection("AlbumProcessedPublisherJob"));
        services.Configure<MongoDbConfig>(configuration.GetSection("MongoDb"));
        services.Configure<MinioFileStorageConfig>(configuration.GetSection("Minio"));

        return services;
    }
}