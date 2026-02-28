using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record BatchSetDecisionDto(List<Guid> Ids, AiVerificationDecision Decision);
