using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Reviews.DeleteReview;

public class DeleteReviewHandler
{
    private readonly CoreDataServiceDbContext _context;

    public DeleteReviewHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Reviews
            .FirstOrDefaultAsync(
                review => review.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _context.Reviews.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
