using FluentValidation;
using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Validators
{
    public class DistributorValidator : AbstractValidator<Distributor>
    {
        public DistributorValidator()
        {
            RuleFor(distributor => distributor.Name)
               .NotEmpty().WithMessage("The distributor name is required.")
               .MaximumLength(100).WithMessage("The distributor name must not exceed 100 characters.");
        }
    }
}
