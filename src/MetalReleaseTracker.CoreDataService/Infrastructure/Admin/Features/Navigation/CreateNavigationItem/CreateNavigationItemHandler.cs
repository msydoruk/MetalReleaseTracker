using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.CreateNavigationItem;

public class CreateNavigationItemHandler
{
    private readonly CoreDataServiceDbContext _context;

    public CreateNavigationItemHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<NavigationItemDto> HandleAsync(
        CreateNavigationItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new NavigationItemEntity
        {
            Id = Guid.NewGuid(),
            Path = request.Path,
            IconName = request.IconName,
            SortOrder = request.SortOrder,
            IsVisible = request.IsVisible,
            IsProtected = request.IsProtected,
            CreatedDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

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

        _context.NavigationItems.Add(entity);
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
