namespace MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;

public interface IHttpRequestService
{
    Task<string> GetStringWithUserAgentAsync(string url, CancellationToken cancellationToken = default);

    Task<string> GetStringWithUserAgentAsync(string url, IDictionary<string, string> additionalHeaders, CancellationToken cancellationToken = default);
}