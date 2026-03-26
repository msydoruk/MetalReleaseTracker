using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbumById;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.UpdateAlbum;

public class UpdateAlbumHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateAlbumHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminAlbumDetailDto?> HandleAsync(
        Guid id,
        UpdateAlbumRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Albums
            .Include(album => album.Band)
            .Include(album => album.Distributor)
            .Include(album => album.Translations)
            .FirstOrDefaultAsync(
                album => album.Id == id,
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

        if (request.Price.HasValue)
        {
            entity.Price = request.Price.Value;
        }

        if (request.Status is not null &&
            Enum.TryParse<AlbumStatus>(request.Status, ignoreCase: true, out var albumStatus))
        {
            entity.Status = albumStatus;
        }

        if (request.StockStatus is not null &&
            Enum.TryParse<AlbumStockStatus>(request.StockStatus, ignoreCase: true, out var stockStatus))
        {
            entity.StockStatus = stockStatus;
        }

        if (request.Description is not null)
        {
            entity.Description = request.Description;
        }

        if (request.Label is not null)
        {
            entity.Label = request.Label;
        }

        if (request.Press is not null)
        {
            entity.Press = request.Press;
        }

        if (request.Translations is not null)
        {
            _context.AlbumTranslations.RemoveRange(entity.Translations);

            foreach (var (languageCode, translationDto) in request.Translations)
            {
                entity.Translations.Add(new AlbumTranslationEntity
                {
                    Id = Guid.NewGuid(),
                    AlbumId = entity.Id,
                    LanguageCode = languageCode,
                    SeoTitle = translationDto.SeoTitle,
                    SeoDescription = translationDto.SeoDescription,
                    SeoKeywords = translationDto.SeoKeywords,
                });
            }
        }

        entity.LastUpdateDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminAlbumDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Genre = entity.Genre,
            Price = entity.Price,
            PurchaseUrl = entity.PurchaseUrl,
            PhotoUrl = entity.PhotoUrl,
            Media = entity.Media?.ToString(),
            Label = entity.Label,
            Press = entity.Press,
            Description = entity.Description,
            CreatedDate = entity.CreatedDate,
            LastUpdateDate = entity.LastUpdateDate,
            Status = entity.Status?.ToString(),
            StockStatus = entity.StockStatus?.ToString(),
            SKU = entity.SKU,
            CanonicalTitle = entity.CanonicalTitle,
            OriginalYear = entity.OriginalYear,
            BandcampUrl = entity.BandcampUrl,
            Slug = entity.Slug,
            BandId = entity.BandId,
            BandName = entity.Band.Name,
            DistributorId = entity.DistributorId,
            DistributorName = entity.Distributor.Name,
            Translations = entity.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new AlbumTranslationDto
                {
                    SeoTitle = translation.SeoTitle,
                    SeoDescription = translation.SeoDescription,
                    SeoKeywords = translation.SeoKeywords,
                }),
        };
    }
}
