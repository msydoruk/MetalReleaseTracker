using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.News.DeleteNewsArticle;

public class DeleteNewsArticleHandler
{
    private readonly CoreDataServiceDbContext _context;

    public DeleteNewsArticleHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.NewsArticles
            .FirstOrDefaultAsync(
                article => article.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _context.NewsArticles.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
