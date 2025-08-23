namespace MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;

public interface IHttpRequestService
{
    Task<string> GetStringWithUserAgentAsync(string url, CancellationToken cancellationToken = default);
}