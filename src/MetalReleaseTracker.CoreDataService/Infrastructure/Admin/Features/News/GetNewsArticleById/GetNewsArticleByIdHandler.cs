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
            .Where(article => article.Id == id)
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
            .FirstOrDefaultAsync(cancellationToken);

        return article;
    }
}
