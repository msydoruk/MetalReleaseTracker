namespace MetalReleaseTracker.CoreDataService.Configuration;

public class KafkaConfig
{
    public string BootstrapServers { get; set; }

    public string CatalogSyncServiceConsumerGroup { get; set; }

    public string CatalogSyncServiceTopic { get; set; }

    public string BandPhotoSyncedTopic { get; set; }

    public string BandPhotoSyncedConsumerGroup { get; set; }

    public KafkaSecurityConfig Security { get; set; }
}