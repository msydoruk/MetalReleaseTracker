using MetalReleaseTracker.ParserService.Infrastructure.Images.Models;

namespace MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;

public interface IImageUploadService
{
    Task<string> UploadAlbumImageAndGetUrlAsync(ImageUploadRequest request, CancellationToken cancellationToken);
}