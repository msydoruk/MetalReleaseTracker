using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record RunVerificationDto(
    DistributorCode? DistributorCode,
    string? Search,
    List<Guid>? CatalogueIndexIds);
