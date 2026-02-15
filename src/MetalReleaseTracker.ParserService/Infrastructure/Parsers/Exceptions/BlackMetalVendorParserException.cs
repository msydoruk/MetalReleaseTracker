namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class BlackMetalVendorParserException : Exception
{
    public BlackMetalVendorParserException() : base()
    {
    }

    public BlackMetalVendorParserException(string message) : base(message)
    {
    }

    public BlackMetalVendorParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
