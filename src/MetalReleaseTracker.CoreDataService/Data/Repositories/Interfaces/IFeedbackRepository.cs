using MetalReleaseTracker.CoreDataService.Data.Entities;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IFeedbackRepository
{
    Task AddAsync(FeedbackEntity entity, CancellationToken cancellationToken = default);
}
