namespace MetalReleaseTracker.ParserService.Infrastructure.Services.Configuration;

public class BandReferenceSettings
{
    public string MetalArchivesBaseUrl { get; set; } = "https://www.metal-archives.com";

    public string SyncCountryCode { get; set; } = "UA";

    public int MinRequestDelayMs { get; set; } = 3000;

    public int MaxRequestDelayMs { get; set; } = 5000;
}
