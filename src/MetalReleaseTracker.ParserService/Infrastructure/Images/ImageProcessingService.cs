using MetalReleaseTracker.ParserService.Infrastructure.Images.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace MetalReleaseTracker.ParserService.Infrastructure.Images;

public class ImageProcessingService : IImageProcessingService
{
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly ImageProcessingSettings _settings;

    public ImageProcessingService(
        IFileStorageService fileStorageService,
        ILogger<ImageProcessingService> logger,
        IOptions<ImageProcessingSettings> options)
    {
        _fileStorageService = fileStorageService;
        _logger = logger;
        _settings = options.Value;
    }

    public async Task GenerateThumbnailsAsync(byte[] imageBytes, string blobPath, CancellationToken cancellationToken = default)
    {
        var basePath = GetBasePathWithoutExtension(blobPath);

        foreach (var width in _settings.ThumbnailWidths)
        {
            try
            {
                await GenerateThumbnailAsync(imageBytes, basePath, width, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Failed to generate {Width}px thumbnail for {BlobPath}",
                    width,
                    blobPath);
            }
        }
    }

    private async Task GenerateThumbnailAsync(byte[] imageBytes, string basePath, int targetWidth, CancellationToken cancellationToken)
    {
        using var image = Image.Load(imageBytes);

        if (image.Width <= targetWidth)
        {
            return;
        }

        var targetHeight = (int)Math.Round((double)image.Height / image.Width * targetWidth);

        image.Mutate(context => context.Resize(targetWidth, targetHeight));

        var thumbnailPath = $"{basePath}_{targetWidth}.webp";

        using var outputStream = new MemoryStream();
        var encoder = new WebpEncoder { Quality = _settings.WebPQuality };
        await image.SaveAsync(outputStream, encoder, cancellationToken);

        outputStream.Position = 0;
        await _fileStorageService.UploadFileAsync(thumbnailPath, outputStream, cancellationToken);

        _logger.LogDebug("Generated {Width}px WebP thumbnail at {ThumbnailPath}", targetWidth, thumbnailPath);
    }

    private static string GetBasePathWithoutExtension(string blobPath)
    {
        var lastDotIndex = blobPath.LastIndexOf('.');
        return lastDotIndex > 0 ? blobPath[..lastDotIndex] : blobPath;
    }
}
