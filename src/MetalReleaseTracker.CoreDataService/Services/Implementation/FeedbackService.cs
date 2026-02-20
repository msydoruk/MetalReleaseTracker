using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Feedback;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;

    public FeedbackService(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task SubmitAsync(SubmitFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new FeedbackEntity
        {
            Id = Guid.NewGuid(),
            Message = request.Message,
            Email = request.Email,
            CreatedDate = DateTime.UtcNow
        };

        await _feedbackRepository.AddAsync(entity, cancellationToken);
    }
}
