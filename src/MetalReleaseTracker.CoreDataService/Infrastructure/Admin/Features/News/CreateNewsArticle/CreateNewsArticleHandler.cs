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
            TitleEn = request.TitleEn,
            TitleUa = request.TitleUa,
            ContentEn = request.ContentEn,
            ContentUa = request.ContentUa,
            ChipLabel = request.ChipLabel,
            ChipColor = request.ChipColor,
            IconName = request.IconName,
            Date = request.Date,
            SortOrder = request.SortOrder,
            IsPublished = request.IsPublished,
            CreatedDate = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _context.NewsArticles.Add(entity);
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
