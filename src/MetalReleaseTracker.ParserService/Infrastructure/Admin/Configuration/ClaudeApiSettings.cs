namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Configuration;

public class ClaudeApiSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string Model { get; set; } = "claude-sonnet-4-20250514";

    public int MaxTokens { get; set; } = 1024;

    public int MaxConcurrentRequests { get; set; } = 5;

    public int DelayBetweenBatchesMs { get; set; } = 1000;
}
