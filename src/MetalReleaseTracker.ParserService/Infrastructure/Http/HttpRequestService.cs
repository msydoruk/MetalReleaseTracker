using Flurl.Http;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.ParserService.Infrastructure.Http;

public class FlurlHttpRequestService : IHttpRequestService
{
    private readonly IUserAgentProvider _userAgentProvider;
    private int _requestTimeoutSeconds;

    public FlurlHttpRequestService(IOptions<HttpRequestSettings> options, IUserAgentProvider userAgentProvider)
    {
        _userAgentProvider = userAgentProvider;
        _requestTimeoutSeconds = options.Value.RequestTimeoutSeconds;
    }

    public async Task<string> GetStringWithUserAgentAsync(string url, CancellationToken cancellationToken = default)
    {
        return await GetStringWithUserAgentAsync(url, new Dictionary<string, string>(), cancellationToken);
    }

    public async Task<string> GetStringWithUserAgentAsync(string url, IDictionary<string, string> additionalHeaders, CancellationToken cancellationToken = default)
    {
        var userAgent = _userAgentProvider.GetRandomUserAgent();

        var request = url.WithTimeout(TimeSpan.FromSeconds(_requestTimeoutSeconds))
            .WithHeader("User-Agent", userAgent);

        foreach (var header in additionalHeaders)
        {
            request = request.WithHeader(header.Key, header.Value);
        }

        var response = await request.GetAsync(cancellationToken: cancellationToken);

        return await response.GetStringAsync();
    }
}