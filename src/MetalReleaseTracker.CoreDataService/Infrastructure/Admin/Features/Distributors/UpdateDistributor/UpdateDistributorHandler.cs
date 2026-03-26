using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.UpdateDistributor;

public class UpdateDistributorHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateDistributorHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDistributorDto?> HandleAsync(
        Guid id,
        UpdateDistributorRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Distributors
            .Include(distributor => distributor.Translations)
            .FirstOrDefaultAsync(
                distributor => distributor.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Name = request.Name;
        if (request.IsVisible.HasValue)
        {
            entity.IsVisible = request.IsVisible.Value;
        }

        entity.Country = request.Country;
        entity.CountryFlag = request.CountryFlag;
        entity.LogoUrl = request.LogoUrl;
        entity.WebsiteUrl = request.WebsiteUrl;

        if (request.Translations is not null)
        {
            _context.DistributorTranslations.RemoveRange(entity.Translations);

            foreach (var (languageCode, translationDto) in request.Translations)
            {
                entity.Translations.Add(new DistributorTranslationEntity
                {
                    Id = Guid.NewGuid(),
                    DistributorId = entity.Id,
                    LanguageCode = languageCode,
                    Description = translationDto.Description,
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var albumCount = await _context.Albums
            .CountAsync(
                album => album.DistributorId == entity.Id,
                cancellationToken);

        return new AdminDistributorDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code.ToString(),
            AlbumCount = albumCount,
            IsVisible = entity.IsVisible,
            Country = entity.Country,
            CountryFlag = entity.CountryFlag,
            LogoUrl = entity.LogoUrl,
            WebsiteUrl = entity.WebsiteUrl,
            Translations = entity.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new DistributorTranslationDto
                {
                    Description = translation.Description,
                }),
        };
    }
}
