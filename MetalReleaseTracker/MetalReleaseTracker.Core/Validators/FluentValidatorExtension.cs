using FluentValidation;
using MetalReleaseTracker.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MetalReleaseTracker.Core.Validators
{
    public static class FluentValidatorExtension
    {
        public static IServiceCollection AddFluentValidators(this IServiceCollection services)
        {
            var validatorTypes = new[]
            {
                typeof(AlbumFilterValidator),
                typeof(AlbumValidator),
                typeof(BandValidator),
                typeof(DistributorValidator),
                typeof(SubscriptionValidator),
                typeof(GuidValidator)
            };

            foreach (var validatorType in validatorTypes)
            {
                var interfaceType = validatorType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
                services.AddTransient(interfaceType, validatorType);
            }

            services.AddTransient<IValidationService, ValidationService>();

            return services;
        }
    }
}
