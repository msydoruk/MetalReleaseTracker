using FluentValidation;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Validators
{
    public class BaseFilterValidator : AbstractValidator<PagingAndSortingFilter>
    {
        public BaseFilterValidator()
        {
            RuleFor(x => x.Take)
                .GreaterThan(0).WithMessage("Take must be greater than 0");

            RuleFor(x => x.Skip)
                .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0");
        }
    }
}
