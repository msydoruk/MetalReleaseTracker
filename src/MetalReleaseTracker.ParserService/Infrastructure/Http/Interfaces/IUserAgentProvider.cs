namespace MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;

public interface IUserAgentProvider
{
    string GetRandomUserAgent();
}