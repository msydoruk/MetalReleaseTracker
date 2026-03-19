using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class ReviewRepository : IReviewRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public ReviewRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ReviewEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Reviews.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ReviewEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Reviews
            .OrderByDescending(review => review.CreatedDate)
            .ToListAsync(cancellationToken);
    }
}
