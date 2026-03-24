using MetalReleaseTracker.CoreDataService.Data;
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
            .FirstOrDefaultAsync(
                article => article.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (request.TitleEn is not null)
        {
            entity.TitleEn = request.TitleEn;
        }

        if (request.TitleUa is not null)
        {
            entity.TitleUa = request.TitleUa;
        }

        if (request.ContentEn is not null)
        {
            entity.ContentEn = request.ContentEn;
        }

        if (request.ContentUa is not null)
        {
            entity.ContentUa = request.ContentUa;
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

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new NewsArticleDto
        {
            Id = entity.Id,
            TitleEn = entity.TitleEn,
            TitleUa = entity.TitleUa,
            ContentEn = entity.ContentEn,
            ContentUa = entity.ContentUa,
            ChipLabel = entity.ChipLabel,
            ChipColor = entity.ChipColor,
            IconName = entity.IconName,
            Date = entity.Date,
            SortOrder = entity.SortOrder,
            IsPublished = entity.IsPublished,
            CreatedDate = entity.CreatedDate,
            UpdatedAt = entity.UpdatedAt,
        };
    }
}
