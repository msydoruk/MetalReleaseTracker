using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;

public interface IAiVerificationService
{
    Task<PagedResultDto<AiVerificationDto>> GetVerificationsAsync(
        AiVerificationFilterDto filter,
        CancellationToken cancellationToken);

    Task<int> RunVerificationAsync(
        DistributorCode? distributorCode,
        CancellationToken cancellationToken);

    Task SetDecisionAsync(
        Guid verificationId,
        AiVerificationDecision decision,
        CancellationToken cancellationToken);

    Task SetBatchDecisionAsync(
        List<Guid> ids,
        AiVerificationDecision decision,
        CancellationToken cancellationToken);
}
