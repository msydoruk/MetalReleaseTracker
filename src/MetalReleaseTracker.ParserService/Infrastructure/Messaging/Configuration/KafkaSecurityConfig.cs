namespace MetalReleaseTracker.ParserService.Infrastructure.Messaging.Configuration;

public class KafkaSecurityConfig
{
    public string Username { get; set; }

    public string Password { get; set; }

    public string Mechanism { get; set; }

    public string SaslProtocol { get; set; }
}