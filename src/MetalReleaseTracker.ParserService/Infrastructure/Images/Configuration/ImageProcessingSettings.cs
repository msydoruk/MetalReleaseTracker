namespace MetalReleaseTracker.ParserService.Infrastructure.Images.Configuration;

public class ImageProcessingSettings
{
    public int[] ThumbnailWidths { get; set; } = [180, 350, 500];

    public int WebPQuality { get; set; } = 80;
}
