using FluentValidation;
using MetalReleaseTracker.Core.Filters;

namespace MetalReleaseTracker.Core.Validators
{
    public class AlbumFilterValidator : AbstractValidator<AlbumFilter>
    {
        public AlbumFilterValidator()
        {
            RuleFor(filter => filter.BandName)
               .MaximumLength(100).WithMessage("Band name cannot be longer than 100 characters.");

            RuleFor(filter => filter.ReleaseDateStart)
                .LessThan(filter => filter.ReleaseDateEnd)
                .When(filter => filter.ReleaseDateStart.HasValue && filter.ReleaseDateEnd.HasValue)
                .WithMessage("ReleaseDateStart must be less than ReleaseDateEnd.");

            RuleFor(filter => filter.MinimumPrice)
                .GreaterThan(0).WithMessage("Minimum price must be greater than 0.")
                .LessThan(filter => filter.MaximumPrice)
                .When(filter => filter.MaximumPrice.HasValue)
                .WithMessage("Minimum price must be less than maximum price.");

            RuleFor(filter => filter.MaximumPrice)
                .GreaterThan(0).WithMessage("Maximum price must be greater than 0.");

            RuleFor(filter => filter.Status)
                .IsInEnum().WithMessage("Status must be a valid AlbumStatus enum value.");

            RuleFor(filter => filter.Media)
               .IsInEnum().WithMessage("Media must be a valid MediaType enum value.");
        }
    }
}
