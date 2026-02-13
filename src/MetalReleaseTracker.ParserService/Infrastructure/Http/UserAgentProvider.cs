using System.Reflection;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Exceptions;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;

namespace MetalReleaseTracker.ParserService.Infrastructure.Http;

public class UserAgentProvider : IUserAgentProvider
{
    private readonly Random _random = new();
    private readonly IReadOnlyList<string> _userAgents;

    public UserAgentProvider()
    {
        _userAgents = LoadUserAgentsFromEmbeddedResource();
    }

    public string GetRandomUserAgent()
    {
        return _userAgents[_random.Next(_userAgents.Count)];
    }

    private static List<string> LoadUserAgentsFromEmbeddedResource()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith("user-agents.txt", StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new UserAgentProviderException("Embedded resource 'user-agents.txt' was not found.");
        }

        try
        {
            using var stream = assembly.GetManifestResourceStream(resourceName)!;
            using var reader = new StreamReader(stream);

            var userAgents = new List<string>();
            while (reader.ReadLine() is { } line)
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    userAgents.Add(line.Trim());
                }
            }

            if (userAgents.Count == 0)
            {
                throw new UserAgentProviderException("Embedded resource 'user-agents.txt' is empty.");
            }

            return userAgents;
        }
        catch (UserAgentProviderException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new UserAgentProviderException("Failed to load user agents from embedded resource.", exception);
        }
    }
}
