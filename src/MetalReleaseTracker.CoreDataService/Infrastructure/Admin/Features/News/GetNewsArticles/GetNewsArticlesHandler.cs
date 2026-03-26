using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.GetNewsArticles;

public class GetNewsArticlesHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetNewsArticlesHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<NewsArticleDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var articles = await _context.NewsArticles
            .AsNoTracking()
            .Include(article => article.Translations)
            .OrderBy(article => article.SortOrder)
            .ToListAsync(cancellationToken);

        return articles.Select(article => new NewsArticleDto
        {
            Id = article.Id,
            ChipLabel = article.ChipLabel,
            ChipColor = article.ChipColor,
            IconName = article.IconName,
            Date = article.Date,
            SortOrder = article.SortOrder,
            IsPublished = article.IsPublished,
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
        }).ToList();
    }
}
