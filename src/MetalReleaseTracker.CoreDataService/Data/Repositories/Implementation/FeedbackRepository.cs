using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Implementation;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly CoreDataServiceDbContext _dbContext;

    public FeedbackRepository(CoreDataServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(FeedbackEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbContext.Feedbacks.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
