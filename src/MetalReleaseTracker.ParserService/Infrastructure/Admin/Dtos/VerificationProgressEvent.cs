namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record VerificationProgressEvent(
    string Type,
    int Processed,
    int Total,
    int Failed,
    string? Current,
    string? Message);
