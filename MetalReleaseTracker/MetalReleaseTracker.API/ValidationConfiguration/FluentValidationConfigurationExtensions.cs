using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Validators;

namespace MetalReleaseTracker.API.ValidationConfiguration
{
    public static class FluentValidationConfigurationExtensions
    {
        public static IServiceCollection AddCustomValidators(this IServiceCollection services)
        {
            services.AddTransient<IValidator<Album>, AlbumValidator>();

            services.AddTransient<IValidator<AlbumFilter>, AlbumFilterValidator>();

            services.AddTransient<IValidator<Band>, BandValidator>();

            services.AddTransient<IValidator<Distributor>, DistributorValidator>();

            services.AddTransient<IValidator<Subscription>, SubscriptionValidator>();

            services.AddTransient<IValidator<Guid>, GuidValidator>();

            return services;
        }

        public static IServiceCollection AddValidationServiceWithAllValidators(this IServiceCollection services)
        {
            services.AddTransient<IValidationService, ValidationService>(serviceProvider =>
            {
                var validators = new List<IValidator>
                {
                    serviceProvider.GetRequiredService<IValidator<Album>>(),

                    serviceProvider.GetRequiredService<IValidator<AlbumFilter>>(),

                    serviceProvider.GetRequiredService<IValidator<Band>>(),

                    serviceProvider.GetRequiredService<IValidator<Distributor>>(),

                    serviceProvider.GetRequiredService<IValidator<Subscription>>(),

                    serviceProvider.GetRequiredService<IValidator<Guid>>()
                };

                return new ValidationService(validators);
            });

            return services;
        }
    }
}
