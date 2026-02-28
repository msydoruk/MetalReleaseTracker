using System.Threading.Channels;
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
        RunVerificationDto request,
        CancellationToken cancellationToken);

    Task RunVerificationStreamAsync(
        RunVerificationDto request,
        ChannelWriter<VerificationProgressEvent> writer,
        CancellationToken cancellationToken);

    Task SetDecisionAsync(
        Guid verificationId,
        AiVerificationDecision decision,
        CancellationToken cancellationToken);

    Task SetBatchDecisionAsync(
        List<Guid> ids,
        AiVerificationDecision decision,
        CancellationToken cancellationToken);

    Task<int> SetBulkDecisionByFilterAsync(
        DistributorCode? distributorCode,
        bool? isUkrainian,
        AiVerificationDecision decision,
        CancellationToken cancellationToken);
}
