using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.Extensions.Caching.Memory;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class ImageUrlResolverService : IImageUrlResolverService
{
    private static readonly int[] ThumbnailWidths = [180, 350, 500];

    private readonly IFileStorageService _fileStorageService;
    private readonly IMemoryCache _memoryCache;

    public ImageUrlResolverService(
        IFileStorageService fileStorageService,
        IMemoryCache memoryCache)
    {
        _fileStorageService = fileStorageService;
        _memoryCache = memoryCache;
    }

    public async Task<ImageUrlSetDto?> ResolveImageUrlSetAsync(string? blobPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(blobPath))
        {
            return null;
        }

        var originalUrl = await _fileStorageService.GetFileUrlAsync(blobPath, cancellationToken);
        if (string.IsNullOrEmpty(originalUrl))
        {
            return null;
        }

        var basePath = GetBasePathWithoutExtension(blobPath);
        var thumbnailPaths = ThumbnailWidths.Select(width => $"{basePath}_{width}.webp").ToArray();

        var existenceResults = await CheckThumbnailExistenceAsync(thumbnailPaths, cancellationToken);

        string? smallUrl = null;
        string? mediumUrl = null;
        string? largeUrl = null;

        if (existenceResults[0])
        {
            smallUrl = await _fileStorageService.GetFileUrlAsync(thumbnailPaths[0], cancellationToken);
        }

        if (existenceResults[1])
        {
            mediumUrl = await _fileStorageService.GetFileUrlAsync(thumbnailPaths[1], cancellationToken);
        }

        if (existenceResults[2])
        {
            largeUrl = await _fileStorageService.GetFileUrlAsync(thumbnailPaths[2], cancellationToken);
        }

        return new ImageUrlSetDto
        {
            Original = originalUrl,
            Small = smallUrl,
            Medium = mediumUrl,
            Large = largeUrl
        };
    }

    private async Task<bool[]> CheckThumbnailExistenceAsync(string[] paths, CancellationToken cancellationToken)
    {
        var tasks = paths.Select(path => CheckCachedExistenceAsync(path, cancellationToken)).ToArray();
        return await Task.WhenAll(tasks);
    }

    private async Task<bool> CheckCachedExistenceAsync(string path, CancellationToken cancellationToken)
    {
        var cacheKey = $"thumb_exists:{path}";

        if (_memoryCache.TryGetValue(cacheKey, out bool cached))
        {
            return cached;
        }

        var exists = await _fileStorageService.FileExistsAsync(path, cancellationToken);
        _memoryCache.Set(cacheKey, exists, TimeSpan.FromHours(1));

        return exists;
    }

    private static string GetBasePathWithoutExtension(string blobPath)
    {
        var lastDotIndex = blobPath.LastIndexOf('.');
        return lastDotIndex > 0 ? blobPath[..lastDotIndex] : blobPath;
    }
}
