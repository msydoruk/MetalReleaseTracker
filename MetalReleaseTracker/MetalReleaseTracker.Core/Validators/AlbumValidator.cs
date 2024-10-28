using FluentValidation;
using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Validators
{
    public class AlbumValidator : AbstractValidator<Album>
    {
        public AlbumValidator()
        {
            RuleFor(album => album.DistributorId)
               .NotEmpty().WithMessage("Distributor ID is required.");

            RuleFor(album => album.BandId)
                .NotEmpty().WithMessage("Band ID is required.");

            RuleFor(album => album.Name)
                .NotEmpty().WithMessage("The album name is required.");

            RuleFor(album => album.ReleaseDate)
                .NotEmpty().WithMessage("The release date is required.")
                .Must(date => date != default(DateTime)).WithMessage("The release date is required.");

            RuleFor(album => album.Media)
                .IsInEnum().WithMessage("Invalid media type.");

            RuleFor(album => album.Label)
                .NotEmpty().WithMessage("The record label name is required.");

            RuleFor(album => album.Press)
                .NotEmpty().WithMessage("The pressing information is required.");

            RuleFor(album => album.Status)
                .IsInEnum().WithMessage("Invalid album status.");
        }
    }
}
