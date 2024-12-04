using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Validators;

namespace MetalReleaseTracker.API.Extensions
{
    public static class FluentValidationConfigurationExtensions
    {
        public static IServiceCollection AddCustomValidators(this IServiceCollection services)
        {
            services.AddTransient<IValidator<Album>, AlbumValidator>();
            services.AddTransient<IValidator<AlbumFilter>, AlbumFilterValidator>();
            services.AddTransient<IValidator<Band>, BandValidator>();
            services.AddTransient<IValidator<BaseFilter>, BaseFilterValidator>();
            services.AddTransient<IValidator<Distributor>, DistributorValidator>();
            services.AddTransient<IValidator<Subscription>, SubscriptionValidator>();
            services.AddTransient<IValidator<Guid>, GuidValidator>();

            return services;
        }

        public static IServiceCollection AddValidationServiceWithAllValidators(this IServiceCollection services)
        {
            services.AddScoped<IEnumerable<IValidator>>(provider =>
            {
                var validators = provider.GetServices<IValidator<Album>>()
                               .Cast<IValidator>()
                               .Concat(provider.GetServices<IValidator<AlbumFilter>>())
                               .Concat(provider.GetServices<IValidator<BaseFilter>>())
                               .Concat(provider.GetServices<IValidator<Band>>())
                               .Concat(provider.GetServices<IValidator<Distributor>>())
                               .Concat(provider.GetServices<IValidator<Subscription>>())
                               .Concat(provider.GetServices<IValidator<Guid>>())
                               .ToList();

                return validators;
            });

            services.AddScoped<IValidationService, ValidationService>();

            return services;
        }
    }
}
