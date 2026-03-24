using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.ParserService.Infrastructure.Jobs;

public class ImageThumbnailBackfillJob
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly ILogger<ImageThumbnailBackfillJob> _logger;

    public ImageThumbnailBackfillJob(
        IFileStorageService fileStorageService,
        IImageProcessingService imageProcessingService,
        ILogger<ImageThumbnailBackfillJob> logger)
    {
        _fileStorageService = fileStorageService;
        _imageProcessingService = imageProcessingService;
        _logger = logger;
    }

    public async Task RunBackfillJob(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting image thumbnail backfill job");

        var stats = new BackfillStats();

        await BackfillPrefixAsync("images/", stats, cancellationToken);
        await BackfillPrefixAsync("band-images/", stats, cancellationToken);

        _logger.LogInformation(
            "Image thumbnail backfill completed: {ProcessedCount} processed, {SkippedCount} skipped, {ErrorCount} errors",
            stats.Processed,
            stats.Skipped,
            stats.Errors);
    }

    private async Task BackfillPrefixAsync(string prefix, BackfillStats stats, CancellationToken cancellationToken)
    {
        var objects = await _fileStorageService.ListObjectsAsync(prefix, cancellationToken);
        var objectSet = new HashSet<string>(objects);
        var originals = objects.Where(IsOriginalImage).ToList();

        _logger.LogInformation("Found {Count} original images under {Prefix}", originals.Count, prefix);

        foreach (var blobPath in originals)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            var basePath = GetBasePathWithoutExtension(blobPath);
            var smallThumbnail = $"{basePath}_180.webp";

            if (objectSet.Contains(smallThumbnail))
            {
                stats.Skipped++;
                continue;
            }

            try
            {
                await using var stream = await _fileStorageService.DownloadFileAsync(blobPath, cancellationToken);
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream, cancellationToken);
                var imageBytes = memoryStream.ToArray();

                await _imageProcessingService.GenerateThumbnailsAsync(imageBytes, blobPath, cancellationToken);
                stats.Processed++;

                if (stats.Processed % 50 == 0)
                {
                    _logger.LogInformation("Backfill progress: {Count} images processed so far", stats.Processed);
                }

                await Task.Delay(100, cancellationToken);
            }
            catch (Exception exception)
            {
                stats.Errors++;
                _logger.LogWarning(exception, "Failed to backfill thumbnails for {BlobPath}", blobPath);
            }
        }
    }

    private static bool IsOriginalImage(string path)
    {
        if (path.Contains("_180.webp") || path.Contains("_350.webp") || path.Contains("_500.webp") || path.Contains("_250.webp"))
        {
            return false;
        }

        var extension = Path.GetExtension(path).ToLowerInvariant();
        return extension is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif";
    }

    private static string GetBasePathWithoutExtension(string blobPath)
    {
        var lastDotIndex = blobPath.LastIndexOf('.');
        return lastDotIndex > 0 ? blobPath[..lastDotIndex] : blobPath;
    }

    private class BackfillStats
    {
        public int Processed { get; set; }

        public int Skipped { get; set; }

        public int Errors { get; set; }
    }
}
