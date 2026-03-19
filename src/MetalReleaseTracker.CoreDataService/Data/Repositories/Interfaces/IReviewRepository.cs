using MetalReleaseTracker.CoreDataService.Data.Entities;

namespace MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;

public interface IReviewRepository
{
    Task AddAsync(ReviewEntity entity, CancellationToken cancellationToken = default);

    Task<List<ReviewEntity>> GetAllAsync(CancellationToken cancellationToken = default);
}
