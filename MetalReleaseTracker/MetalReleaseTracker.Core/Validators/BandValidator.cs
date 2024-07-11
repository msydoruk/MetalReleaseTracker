using FluentValidation;
using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Validators
{
    public class BandValidator : AbstractValidator<Band>
    {
        public BandValidator()
        {
            RuleFor(band => band.Name)
                .NotEmpty().WithMessage("The band name is required.")
                .MaximumLength(100).WithMessage("The band name must not exceed 100 characters.");
        }
    }
}
