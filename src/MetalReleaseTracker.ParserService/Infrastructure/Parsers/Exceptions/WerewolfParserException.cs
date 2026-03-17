namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class WerewolfParserException : Exception
{
    public WerewolfParserException() : base()
    {
    }

    public WerewolfParserException(string message) : base(message)
    {
    }

    public WerewolfParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
