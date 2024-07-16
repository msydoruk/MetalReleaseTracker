using FluentValidation;

namespace MetalReleaseTracker.Core.Validators
{
    public class UniversalValidator
    {
        private readonly IServiceProvider _serviceProvider;

        public UniversalValidator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void Validate<T>(T entity)
        {
            var validatorType = typeof(IValidator<T>);
            var validator = _serviceProvider.GetService(validatorType) as IValidator<T>;
            if (validator == null)
            {
                throw new InvalidOperationException($"No validator found for type {typeof(T).Name}");
            }

            var result = validator.Validate(entity);
            if (!result.IsValid)
            {
                throw new ValidationException(result.Errors);
            }
        }
    }
}
