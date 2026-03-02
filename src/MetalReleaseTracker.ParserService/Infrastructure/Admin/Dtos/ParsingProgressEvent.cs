using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record ParsingProgressEvent(
    ParsingEventType Type,
    Guid RunId,
    ParsingJobType JobType,
    DistributorCode DistributorCode,
    int Processed,
    int Total,
    int Failed,
    string? CurrentItem,
    string? Message,
    DateTime Timestamp);
