using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.UpdateNewsArticle;

public class UpdateNewsArticleHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateNewsArticleHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<NewsArticleDto?> HandleAsync(
        Guid id,
        UpdateNewsArticleRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.NewsArticles
            .Include(article => article.Translations)
            .FirstOrDefaultAsync(
                article => article.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (request.ChipLabel is not null)
        {
            entity.ChipLabel = request.ChipLabel;
        }

        if (request.ChipColor is not null)
        {
            entity.ChipColor = request.ChipColor;
        }

        if (request.IconName is not null)
        {
            entity.IconName = request.IconName;
        }

        if (request.Date.HasValue)
        {
            entity.Date = request.Date.Value;
        }

        if (request.SortOrder.HasValue)
        {
            entity.SortOrder = request.SortOrder.Value;
        }

        if (request.IsPublished.HasValue)
        {
            entity.IsPublished = request.IsPublished.Value;
        }

        if (request.Translations is not null)
        {
            _context.NewsArticleTranslations.RemoveRange(entity.Translations.ToList());
            entity.Translations.Clear();

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
        }

        entity.UpdatedAt = DateTime.UtcNow;
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
