using FluentValidation;
using MetalReleaseTracker.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MetalReleaseTracker.Core.Validators
{
    public static class FluentValidatorExtension
    {
        public static IServiceCollection AddFluentValidators(this IServiceCollection services)
        {
            var assembly = typeof(FluentValidatorExtension).Assembly;

            var validatorTypes = assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
                .ToList();

            foreach (var validatorType in validatorTypes)
            {
                var interfaceType = validatorType.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>));
                services.AddTransient(interfaceType, validatorType);
            }

            services.AddTransient<IValidationService, ValidationService>(serviceProvider =>
            {
                var validators = validatorTypes.Select(vt => serviceProvider.GetService(vt.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))).Cast<IValidator>().ToList();
                return new ValidationService(validators);
            });

            return services;
        }
    }
}
