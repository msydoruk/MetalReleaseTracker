using MetalReleaseTracker.ParserService.Domain.Models.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Jobs.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.ParserService.Infrastructure.Common.Extensions;

public static class AppSettingsRegistrationExtension
{
    public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<GeneralParserSettings>(configuration.GetSection("GeneralParserSettings"));
        services.Configure<HttpRequestSettings>(configuration.GetSection("HttpRequestSettings"));
        services.Configure<AlbumParsedPublisherJobSettings>(configuration.GetSection("AlbumParsedPublisherJob"));
        services.Configure<List<ParserDataSource>>(configuration.GetSection("ParserDataSources"));
        services.Configure<MinioFileStorageConfig>(configuration.GetSection("Minio"));

        return services;
    }
}