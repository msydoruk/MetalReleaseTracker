using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.CreateNewsArticle;

public class CreateNewsArticleHandler
{
    private readonly CoreDataServiceDbContext _context;

    public CreateNewsArticleHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<NewsArticleDto> HandleAsync(
        CreateNewsArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = new NewsArticleEntity
        {
            Id = Guid.NewGuid(),
            ChipLabel = request.ChipLabel,
            ChipColor = request.ChipColor,
            IconName = request.IconName,
            Date = request.Date,
            SortOrder = request.SortOrder,
            IsPublished = request.IsPublished,
            ScheduledPublishDate = request.ScheduledPublishDate,
            CreatedDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        foreach (var (languageCode, translationDto) in request.Translations)
        {
            entity.Translations.Add(new NewsArticleTranslationEntity
            {
                Id = Guid.NewGuid(),
                NewsArticleId = entity.Id,
                LanguageCode = languageCode,
                Title = translationDto.Title,
                Content = translationDto.Content,
                SeoTitle = translationDto.SeoTitle,
                SeoDescription = translationDto.SeoDescription,
                SeoKeywords = translationDto.SeoKeywords,
            });
        }

        _context.NewsArticles.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new NewsArticleDto
        {
            Id = entity.Id,
            ChipLabel = entity.ChipLabel,
            ChipColor = entity.ChipColor,
            IconName = entity.IconName,
            Date = entity.Date,
            SortOrder = entity.SortOrder,
            IsPublished = entity.IsPublished,
            ScheduledPublishDate = entity.ScheduledPublishDate,
            CreatedDate = entity.CreatedDate,
            UpdatedAt = entity.UpdatedAt,
            Translations = entity.Translations.ToDictionary(
                translation => translation.LanguageCode,
                translation => new NewsArticleTranslationDto
                {
                    Title = translation.Title,
                    Content = translation.Content,
                    SeoTitle = translation.SeoTitle,
                    SeoDescription = translation.SeoDescription,
                    SeoKeywords = translation.SeoKeywords,
                }),
        };
    }
}
