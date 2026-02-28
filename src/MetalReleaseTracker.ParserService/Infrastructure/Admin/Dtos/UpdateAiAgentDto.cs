namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record UpdateAiAgentDto(
    string Name,
    string? Description,
    string SystemPrompt,
    string Model,
    int MaxTokens,
    int MaxConcurrentRequests,
    int DelayBetweenBatchesMs,
    string? ApiKey,
    bool IsActive);
