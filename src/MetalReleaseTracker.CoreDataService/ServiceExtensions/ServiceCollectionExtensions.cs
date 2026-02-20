using MetalReleaseTracker.CoreDataService.Data.MappingProfiles;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Implementation;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.CoreDataService.ServiceExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAutoMapper(typeof(MappingProfile));
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddMinio();
            services.AddKafka(configuration);

            services.AddCommonServices()
                .AddRepositories()
                .AddDomainServices()
                .AddAuthServices();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<IBandRepository, BandRepository>();
            services.AddScoped<IDistributorsRepository, DistributorRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserFavoriteRepository, UserFavoriteRepository>();

            return services;
        }

        private static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IBandService, BandService>();
            services.AddScoped<IDistributorService, DistributorService>();
            services.AddScoped<IUserFavoriteService, UserFavoriteService>();

            return services;
        }

        private static IServiceCollection AddAuthServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            return services;
        }

        private static IServiceCollection AddCommonServices(this IServiceCollection services)
        {
            services.AddScoped<IFileStorageService, MinioFileStorageService>();
            return services;
        }
    }
}