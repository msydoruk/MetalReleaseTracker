namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class NapalmRecordsParserException : Exception
{
    public NapalmRecordsParserException() : base()
    {
    }

    public NapalmRecordsParserException(string message) : base(message)
    {
    }

    public NapalmRecordsParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
