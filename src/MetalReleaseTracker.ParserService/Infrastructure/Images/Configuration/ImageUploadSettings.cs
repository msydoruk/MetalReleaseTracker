namespace MetalReleaseTracker.ParserService.Infrastructure.Images.Configuration;

public class ImageUploadSettings
{
    public int RequestTimeoutSeconds { get; set; }

    public int MaxImageSizeBytes { get; set; }

    public int MinImageSizeBytes { get; set; }

    public string ImageStorageFolder { get; set; }

    public string TimestampFormat { get; set; }

    public string DefaultFileName { get; set; }

    public string DefaultImageExtension { get; set; }

    public Dictionary<string, string> SupportedExtensions { get; set; } = new();
}