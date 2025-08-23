using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.Extensions.Options;
using Minio;

namespace MetalReleaseTracker.CatalogSyncService.ServiceExtensions;

public static class MinioRegistrationExtension
{
    public static IServiceCollection AddMinio(this IServiceCollection services)
    {
        services.AddSingleton<IMinioClient>(serviceProvider =>
        {
            var minioConfig = serviceProvider.GetRequiredService<IOptions<MinioFileStorageConfig>>().Value;

            return new MinioClient()
                .WithEndpoint(minioConfig.Endpoint)
                .WithCredentials(minioConfig.AccessKey, minioConfig.SecretKey)
                .Build();
        });

        return services;
    }
}