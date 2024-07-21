using FluentValidation;

namespace MetalReleaseTracker.Core.Validators
{
    public static class FluentValidationConfigurationExtensions
    {
        public static IEnumerable<IValidator> GetFluentValidators(IServiceProvider serviceProvider)
        {
            var assembly = typeof(FluentValidationConfigurationExtensions).Assembly;

            var validatorTypes = assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)))
                .ToList();

            var validators = validatorTypes
               .Select(vt => serviceProvider
               .GetService(vt.GetInterfaces()
               .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>))))
               .Cast<IValidator>()
               .ToList();

            return validators;
        }
    }
}
