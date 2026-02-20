using MetalReleaseTracker.CoreDataService.Services.Dtos.Feedback;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IFeedbackService
{
    Task SubmitAsync(SubmitFeedbackRequest request, CancellationToken cancellationToken = default);
}
