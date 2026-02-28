namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class AiAgentDto
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public string? Description { get; set; }

    public string SystemPrompt { get; set; }

    public string Model { get; set; }

    public int MaxTokens { get; set; }

    public int MaxConcurrentRequests { get; set; }

    public int DelayBetweenBatchesMs { get; set; }

    public bool HasApiKey { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
