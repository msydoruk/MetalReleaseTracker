using Flurl.Http;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.ParserService.Infrastructure.Http;

public class FlurlHttpRequestService : IHttpRequestService
{
    //private readonly IUserAgentProvider _userAgentProvider;
    private int _requestTimeoutSeconds;

    public FlurlHttpRequestService(IOptions<HttpRequestSettings> options)
    {
        //_userAgentProvider = userAgentProvider;
        _requestTimeoutSeconds = options.Value.RequestTimeoutSeconds;
    }

    public async Task<string> GetStringWithUserAgentAsync(string url, CancellationToken cancellationToken = default)
    {
        //var userAgent = _userAgentProvider.GetRandomUserAgent();

        var response = await url.WithTimeout(TimeSpan.FromSeconds(_requestTimeoutSeconds))
            //.WithHeader("User-Agent", userAgent)
            .GetAsync(cancellationToken: cancellationToken);

        return await response.GetStringAsync();
    }
}