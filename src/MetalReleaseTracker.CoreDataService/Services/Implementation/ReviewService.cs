using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Review;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;

    public ReviewService(IReviewRepository reviewRepository)
    {
        _reviewRepository = reviewRepository;
    }

    public async Task SubmitAsync(string userName, SubmitReviewRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new ReviewEntity
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Message = request.Message,
            CreatedDate = DateTime.UtcNow,
        };

        await _reviewRepository.AddAsync(entity, cancellationToken);
    }

    public async Task<List<ReviewDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _reviewRepository.GetAllAsync(cancellationToken);

        return entities.Select(entity => new ReviewDto
        {
            Id = entity.Id,
            UserName = entity.UserName,
            Message = entity.Message,
            CreatedDate = entity.CreatedDate,
        }).ToList();
    }
}
