namespace MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;

public interface IImageProcessingService
{
    Task GenerateThumbnailsAsync(byte[] imageBytes, string blobPath, CancellationToken cancellationToken = default);
}
