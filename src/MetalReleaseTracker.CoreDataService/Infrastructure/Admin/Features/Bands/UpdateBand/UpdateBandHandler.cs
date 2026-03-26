using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.UpdateBand;

public class UpdateBandHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateBandHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminBandDto?> HandleAsync(
        Guid id,
        UpdateBandRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Bands
            .Include(band => band.Translations)
            .FirstOrDefaultAsync(
                band => band.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (request.Name is not null)
        {
            entity.Name = request.Name;
        }

        if (request.Genre is not null)
        {
            entity.Genre = request.Genre;
        }

        if (request.PhotoUrl is not null)
        {
            entity.PhotoUrl = request.PhotoUrl;
        }

        if (request.MetalArchivesUrl is not null)
        {
            entity.MetalArchivesUrl = request.MetalArchivesUrl;
        }

        if (request.FormationYear is not null)
        {
            entity.FormationYear = request.FormationYear;
        }

        if (request.IsVisible.HasValue)
        {
            entity.IsVisible = request.IsVisible.Value;
        }

        if (request.Translations is not null)
        {
            _context.BandTranslations.RemoveRange(entity.Translations);

            foreach (var (languageCode, translationDto) in request.Translations)
            {
                entity.Translations.Add(new BandTranslationEntity
                {
                    Id = Guid.NewGuid(),
                    BandId = entity.Id,
                    LanguageCode = languageCode,
                    Description = translationDto.Description,
                    SeoTitle = translationDto.SeoTitle,
                    SeoDescription = translationDto.SeoDescription,
                    SeoKeywords = translationDto.SeoKeywords,
                });
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var albumCount = await _context.Albums
            .CountAsync(
                album => album.BandId == entity.Id,
                cancellationToken);

        return new AdminBandDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Genre = entity.Genre,
            PhotoUrl = entity.PhotoUrl,
            MetalArchivesUrl = entity.MetalArchivesUrl,
            FormationYear = entity.FormationYear,
            AlbumCount = albumCount,
            Slug = entity.Slug,
            IsVisible = entity.IsVisible,
            Translations = entity.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new BandTranslationDto
                {
                    Description = translation.Description,
                    SeoTitle = translation.SeoTitle,
                    SeoDescription = translation.SeoDescription,
                    SeoKeywords = translation.SeoKeywords,
                }),
        };
    }
}
