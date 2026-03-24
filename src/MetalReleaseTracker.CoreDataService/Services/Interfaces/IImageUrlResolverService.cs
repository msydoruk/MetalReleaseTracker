using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IImageUrlResolverService
{
    Task<ImageUrlSetDto?> ResolveImageUrlSetAsync(string? blobPath, CancellationToken cancellationToken = default);
}
