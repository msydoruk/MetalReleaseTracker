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
            .OrderBy(article => article.SortOrder)
            .Select(article => new NewsArticleDto
            {
                Id = article.Id,
                TitleEn = article.TitleEn,
                TitleUa = article.TitleUa,
                ContentEn = article.ContentEn,
                ContentUa = article.ContentUa,
                ChipLabel = article.ChipLabel,
                ChipColor = article.ChipColor,
                IconName = article.IconName,
                Date = article.Date,
                SortOrder = article.SortOrder,
                IsPublished = article.IsPublished,
                CreatedDate = article.CreatedDate,
                UpdatedAt = article.UpdatedAt,
            })
            .ToListAsync(cancellationToken);

        return articles;
    }
}
