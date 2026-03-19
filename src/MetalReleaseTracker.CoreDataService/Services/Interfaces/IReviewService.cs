using MetalReleaseTracker.CoreDataService.Services.Dtos.Review;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IReviewService
{
    Task SubmitAsync(string userName, SubmitReviewRequest request, CancellationToken cancellationToken = default);

    Task<List<ReviewDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
