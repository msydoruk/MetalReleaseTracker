using FluentValidation;
using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace MetalReleaseTracker.Core.Validators
{
    public static class FluentValidationConfigurationExtensions
    {
        public static IEnumerable<IValidator> GetFluentValidators(IServiceProvider serviceProvider)
        {
            var validators = new List<IValidator>
            {
                serviceProvider.GetService<IValidator<AlbumFilter>>(),

                serviceProvider.GetService<IValidator<Album>>(),

                serviceProvider.GetService<IValidator<Band>>(),

                serviceProvider.GetService<IValidator<Distributor>>(),

                serviceProvider.GetService<IValidator<Subscription>>(),

                serviceProvider.GetService<IValidator<Guid>>()
            };

            return validators;
        }
    }
}
