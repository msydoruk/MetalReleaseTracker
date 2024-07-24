using FluentValidation;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Core.Validators
{
    public class ValidationService : IValidationService
    {
        private readonly IEnumerable<IValidator> _validators;

        public ValidationService(IEnumerable<IValidator> validators)
        {
            _validators = validators;
        }

        public void Validate<T>(T entity)
        {
            var validator = _validators.OfType<IValidator<T>>().FirstOrDefault();
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
