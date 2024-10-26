using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Services;
using MetalReleaseTracker.Infrastructure.Data.MappingProfiles;
using MetalReleaseTracker.Infrastructure.Repositories;

using Microsoft.Extensions.DependencyInjection;

namespace MetalReleaseTracker.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedServices(this IServiceCollection services)
        {
            services.AddScoped<IAlbumRepository, AlbumRepository>();
            services.AddScoped<IBandRepository, BandRepository>();
            services.AddScoped<IDistributorsRepository, DistributorsRepository>();
            services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

            services.AddScoped<IAlbumService, AlbumService>();
            services.AddScoped<IBandService, BandService>();
            services.AddScoped<IDistributorsService, DistributorsService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            
            services.AddCustomValidators();
            services.AddValidationServiceWithAllValidators();

            services.AddAutoMapper(typeof(MappingProfile));

            return services;
        }
    }
}