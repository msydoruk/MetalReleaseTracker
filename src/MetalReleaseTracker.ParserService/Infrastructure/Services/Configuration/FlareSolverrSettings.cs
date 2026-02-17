namespace MetalReleaseTracker.ParserService.Infrastructure.Services.Configuration;

public class FlareSolverrSettings
{
    public string BaseUrl { get; set; } = "http://flaresolverr:8191";

    public int MaxTimeoutMs { get; set; } = 60000;
}
