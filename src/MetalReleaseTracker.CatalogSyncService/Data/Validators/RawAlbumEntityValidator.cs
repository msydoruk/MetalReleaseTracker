using FluentValidation;
using MetalReleaseTracker.CatalogSyncService.Data.Entities;

namespace MetalReleaseTracker.CatalogSyncService.Data.Validators;

public class RawAlbumEntityValidator : AbstractValidator<RawAlbumEntity>
{
    public RawAlbumEntityValidator()
    {
        RuleFor(album => album.BandName)
            .NotEmpty().WithMessage("Band name is required.");

        RuleFor(album => album.Name)
            .NotEmpty().WithMessage("Name is required.");

        RuleFor(album => album.SKU)
            .NotEmpty().WithMessage("SKU is required.");

        RuleFor(album => album.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(album => album.Media)
            .NotEmpty().WithMessage("Media is required.");
    }
}