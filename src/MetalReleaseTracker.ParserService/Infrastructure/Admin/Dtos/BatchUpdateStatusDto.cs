using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record BatchUpdateStatusDto(List<Guid> Ids, CatalogueIndexStatus Status);
