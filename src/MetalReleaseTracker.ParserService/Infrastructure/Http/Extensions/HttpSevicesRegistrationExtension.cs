using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Interfaces;

namespace MetalReleaseTracker.ParserService.Infrastructure.Http.Extensions;

public static class HttpSevicesRegistrationExtension
{
    public static IServiceCollection AddHttpServices(this IServiceCollection services)
    {
        services.AddSingleton<IUserAgentProvider, UserAgentProvider>();
        services.AddSingleton<IHttpRequestService, FlurlHttpRequestService>();

        return services;
    }
}