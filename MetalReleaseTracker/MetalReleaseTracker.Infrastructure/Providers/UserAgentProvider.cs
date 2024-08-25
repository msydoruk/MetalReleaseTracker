using System;

namespace MetalReleaseTracker.Infrastructure.Http
{
    public class UserAgentProvider
    {
        private readonly Random _random;
        private readonly List<string> _userAgents;

        public UserAgentProvider(string filePath)
        {
            _random = new Random();
            _userAgents = LoadUserAgentsFromFile(filePath);
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