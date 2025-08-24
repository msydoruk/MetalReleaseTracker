using MetalReleaseTracker.ParserService.Infrastructure.Images.Models;

namespace MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;

public interface IImageUploadService
{
    Task<ImageUploadResult> UploadAlbumImageAsync(ImageUploadRequest request, CancellationToken cancellationToken);
}