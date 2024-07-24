using FluentValidation;

namespace MetalReleaseTracker.Core.Validators
{
    public class GuidValidator : AbstractValidator<Guid>
    {
        public GuidValidator()
        {
            RuleFor(guid => guid).NotEqual(Guid.Empty).WithMessage("The ID must be a non-empty GUID.");
        }
    }
}
