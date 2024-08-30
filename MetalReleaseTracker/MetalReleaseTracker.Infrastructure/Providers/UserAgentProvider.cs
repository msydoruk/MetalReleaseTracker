using Microsoft.Extensions.Configuration;

namespace MetalReleaseTracker.Infrastructure.Providers
{
    public class UserAgentProvider
    {
        private readonly Random _random;
        private readonly List<string> _userAgents;

        public UserAgentProvider(IConfiguration configuration)
        {
            _random = new Random();

            var relativeFilePath = configuration.GetValue<string>("FileSettings:UserAgentsFilePath");
            var absoluteFilePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, relativeFilePath));

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
            catch (Exception exception)
            {
                throw new InvalidOperationException("Could not load User-Agent from file.", exception);
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