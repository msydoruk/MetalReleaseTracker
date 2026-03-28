using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticleById;

public class GetNewsArticleByIdHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetNewsArticleByIdHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<NewsArticleDto?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var article = await _context.NewsArticles
            .AsNoTracking()
            .Include(article => article.Translations)
            .FirstOrDefaultAsync(article => article.Id == id, cancellationToken);

        if (article is null)
        {
            return null;
        }

        return new NewsArticleDto
        {
            Id = article.Id,
            ChipLabel = article.ChipLabel,
            ChipColor = article.ChipColor,
            IconName = article.IconName,
            Date = article.Date,
            SortOrder = article.SortOrder,
            IsPublished = article.IsPublished,
            ScheduledPublishDate = article.ScheduledPublishDate,
            CreatedDate = article.CreatedDate,
            UpdatedAt = article.UpdatedAt,
            Translations = article.Translations.ToDictionary(
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
