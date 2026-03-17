namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class AvantgardeMusicParserException : Exception
{
    public AvantgardeMusicParserException() : base()
    {
    }

    public AvantgardeMusicParserException(string message) : base(message)
    {
    }

    public AvantgardeMusicParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
