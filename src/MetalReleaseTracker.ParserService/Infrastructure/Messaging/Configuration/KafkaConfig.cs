namespace MetalReleaseTracker.ParserService.Infrastructure.Messaging.Configuration;

public class KafkaConfig
{
    public string BootstrapServers { get; set; }

    public string AlbumProcessedTopic { get; set; }

    public string BandPhotoSyncedTopic { get; set; }

    public KafkaSecurityConfig Security { get; set; }
}
