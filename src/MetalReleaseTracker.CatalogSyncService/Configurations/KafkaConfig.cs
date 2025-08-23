namespace MetalReleaseTracker.CatalogSyncService.Configurations;

public class KafkaConfig
{
    public string BootstrapServers { get; set; }

    public string ParserServiceConsumerGroup { get; set; }

    public string ParserServiceTopic { get; set; }

    public string CatalogSyncServiceTopic { get; set; }

    public KafkaSecurityConfig Security { get; set; }
}