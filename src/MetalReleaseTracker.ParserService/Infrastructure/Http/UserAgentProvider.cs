using MetalReleaseTracker.ParserService.Infrastructure.Http.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.ParserService.Infrastructure.Http;

public class UserAgentProvider : IUserAgentProvider
{
    private readonly Random _random = new();
    private readonly IReadOnlyList<string> _userAgents;
    private readonly IFileStorageService _fileStorageService;

    public UserAgentProvider(IOptions<HttpRequestSettings> options, IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
        _userAgents = InitializeUserAgents(options.Value.UserAgentsFilePath);
    }

    public string GetRandomUserAgent()
    {
        return _userAgents[_random.Next(_userAgents.Count)];
    }

    private List<string> InitializeUserAgents(string filePath)
    {
        try
        {
            return _fileStorageService.DownloadFileAsListAsync(filePath).GetAwaiter().GetResult();
        }
        catch (Exception exception)
        {
            throw new UserAgentProviderException("Failed to load user agents from the specified file.", exception);
        }
    }
}