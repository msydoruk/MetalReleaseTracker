namespace MetalReleaseTracker.ParserService.Infrastructure.Http.Exceptions;

public class UserAgentProviderException : Exception
{
    public UserAgentProviderException() : base()
    {
    }

    public UserAgentProviderException(string message) : base(message)
    {
    }

    public UserAgentProviderException(string message, Exception innerException) : base(message, innerException)
    {
    }
}