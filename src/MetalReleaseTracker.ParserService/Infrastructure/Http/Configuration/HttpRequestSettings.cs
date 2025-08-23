namespace MetalReleaseTracker.ParserService.Infrastructure.Http.Configuration;

public class HttpRequestSettings
{
    public string UserAgentsFilePath { get; set; } = string.Empty;

    public int RequestTimeoutSeconds { get; set; } = 30;
}