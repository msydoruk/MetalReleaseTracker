using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MetalReleaseTracker.Infrastructure.Http
{
    public class UserAgentProvider
    {
        private readonly Random _random;
        private readonly List<string> _userAgents;

        public UserAgentProvider(IConfiguration configuration)
        {
            _random = new Random();

            var relativeFilePath = configuration.GetValue<string>("FileSettings:UserAgentsFilePath");

            var basePath = AppContext.BaseDirectory;

            var infrastructurePath = Path.GetFullPath(Path.Combine(basePath, @"..\..\..\..\MetalReleaseTracker.Infrastructure"));
            var absoluteFilePath = Path.Combine(infrastructurePath, relativeFilePath);

            _userAgents = LoadUserAgentsFromFile(absoluteFilePath);
        }

        private List<string> LoadUserAgentsFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return new List<string>(File.ReadAllLines(filePath));
                }
                else
                {
                    throw new FileNotFoundException($"File with User-Agent not found: {filePath}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not load User-Agent from file.", ex);
            }
        }

        public string GetRandomUserAgent()
        {
            if (_userAgents.Count == 0)
            {
                throw new InvalidOperationException("The User-Agent list is empty.");
            }

            int index = _random.Next(_userAgents.Count);
            return _userAgents[index];
        }
    }
}