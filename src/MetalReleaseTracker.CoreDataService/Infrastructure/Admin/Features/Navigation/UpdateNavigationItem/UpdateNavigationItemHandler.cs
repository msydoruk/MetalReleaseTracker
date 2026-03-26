using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.UpdateNavigationItem;

public class UpdateNavigationItemHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateNavigationItemHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<NavigationItemDto?> HandleAsync(
        Guid id,
        UpdateNavigationItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.NavigationItems
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(
                item => item.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (request.Path is not null)
        {
            entity.Path = request.Path;
        }

        if (request.IconName is not null)
        {
            entity.IconName = request.IconName;
        }

        if (request.SortOrder.HasValue)
        {
            entity.SortOrder = request.SortOrder.Value;
        }

        if (request.IsVisible.HasValue)
        {
            entity.IsVisible = request.IsVisible.Value;
        }

        if (request.IsProtected.HasValue)
        {
            entity.IsProtected = request.IsProtected.Value;
        }

        if (request.Translations is not null)
        {
            _context.NavigationItemTranslations.RemoveRange(entity.Translations.ToList());
            entity.Translations.Clear();

            foreach (var (languageCode, translationDto) in request.Translations)
            {
                entity.Translations.Add(new NavigationItemTranslationEntity
                {
                    Id = Guid.NewGuid(),
                    NavigationItemId = entity.Id,
                    LanguageCode = languageCode,
                    Title = translationDto.Title,
                    SeoTitle = translationDto.SeoTitle,
                    SeoDescription = translationDto.SeoDescription,
                    SeoKeywords = translationDto.SeoKeywords,
                });
            }
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new NavigationItemDto
        {
            Id = entity.Id,
            Path = entity.Path,
            IconName = entity.IconName,
            SortOrder = entity.SortOrder,
            IsVisible = entity.IsVisible,
            IsProtected = entity.IsProtected,
            CreatedDate = entity.CreatedDate,
            UpdatedAt = entity.UpdatedAt,
            Translations = entity.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new NavigationItemTranslationDto
                {
                    Title = translation.Title,
                    SeoTitle = translation.SeoTitle,
                    SeoDescription = translation.SeoDescription,
                    SeoKeywords = translation.SeoKeywords,
                }),
        };
    }
}
