using MetalReleaseTracker.CoreDataService.Data.MappingProfiles;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Implementation;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;

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
            services.AddKafka(configuration);

            services.RegisterRepositories()
                .RegisterDomainServices()
                .AddAuthServices();

            return services;
        }

        private static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<IBandRepository, BandRepository>();
            services.AddScoped<IDistributorsRepository, DistributorRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            return services;
        }

        private static IServiceCollection RegisterDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IBandService, BandService>();
            services.AddScoped<IDistributorService, DistributorService>();

            return services;
        }

        private static IServiceCollection AddAuthServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            return services;
        }
    }
}