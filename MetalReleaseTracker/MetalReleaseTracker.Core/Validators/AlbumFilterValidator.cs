using FluentValidation;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Validators
{
    public class AlbumFilterValidator : AbstractValidator<AlbumFilter>
    {
        public AlbumFilterValidator()
        {
            RuleFor(filter => filter.BandName)
               .NotEmpty().WithMessage("Band name is required.")
               .MaximumLength(100).WithMessage("Band name cannot be longer than 100 characters.");

            RuleFor(filter => filter.ReleaseDateStart)
                .LessThanOrEqualTo(filter => filter.ReleaseDateEnd)
                .When(filter => filter.ReleaseDateStart.HasValue && filter.ReleaseDateEnd.HasValue)
                .WithMessage("ReleaseDateStart must be less than or equal to ReleaseDateEnd.");

            RuleFor(filter => filter.Genre)
                .NotEmpty().WithMessage("Genre is required.")
                .MaximumLength(50).WithMessage("Genre cannot be longer than 50 characters.");

            RuleFor(filter => filter.MinimumPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum price must be greater than or equal to 0.")
                .LessThanOrEqualTo(filter => filter.MaximumPrice)
                .When(filter => filter.MaximumPrice.HasValue)
                .WithMessage("Minimum price must be less than or equal to maximum price.");

            RuleFor(filter => filter.MaximumPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Maximum price must be greater than or equal to 0.");

            RuleFor(filter => filter.Status)
                .IsInEnum().WithMessage("Status must be a valid AlbumStatus enum value.");
        }
    }
}
