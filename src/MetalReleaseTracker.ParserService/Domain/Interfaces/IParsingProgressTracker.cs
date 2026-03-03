using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface IParsingProgressTracker
{
    void StartRun(Guid runId, ParsingJobType jobType, DistributorCode code, int totalItems);

    void StartRun(Guid runId, ParsingJobType jobType, DistributorCode code);

    void StartRun(Guid runId, ParsingJobType jobType, int totalItems);

    void StartRun(Guid runId, ParsingJobType jobType);

    void ItemProcessed(Guid runId, string itemDescription, params string[] categories);

    void ItemFailed(Guid runId, string itemDescription, string error, params string[] categories);

    void CompleteRun(Guid runId);

    void FailRun(Guid runId, string errorMessage);

    IAsyncEnumerable<ParsingProgressEvent> SubscribeAsync(CancellationToken cancellationToken);
}
