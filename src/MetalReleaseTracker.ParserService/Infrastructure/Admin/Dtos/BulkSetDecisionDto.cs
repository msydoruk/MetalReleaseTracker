using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record BulkSetDecisionDto(DistributorCode? DistributorCode, bool? IsUkrainian, AiVerificationDecision Decision);
