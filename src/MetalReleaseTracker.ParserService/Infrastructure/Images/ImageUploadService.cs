using FileTypeChecker.Extensions;
using Flurl.Http;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Http.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Images.Models;
using MetalReleaseTracker.SharedLibraries.Minio;
using Microsoft.Extensions.Options;

namespace MetalReleaseTracker.ParserService.Infrastructure.Images;

public class ImageUploadService : IImageUploadService
{
    private readonly IUserAgentProvider _userAgentProvider;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<ImageUploadService> _logger;
    private readonly ImageUploadSettings _settings;

    public ImageUploadService(
        IUserAgentProvider userAgentProvider,
        IFileStorageService fileStorageService,
        ILogger<ImageUploadService> logger,
        IOptions<ImageUploadSettings> options)
    {
        _userAgentProvider = userAgentProvider;
        _fileStorageService = fileStorageService;
        _logger = logger;
        _settings = options.Value;
    }

    public async Task<ImageUploadResult> UploadAlbumImageAsync(ImageUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.ImageUrl))
        {
            return ImageUploadResult.Failure("Image URL is empty", request.ImageUrl);
        }

        var blobPath = GenerateBlobPath(request.DistributorCode, request.AlbumSku, request.ImageUrl);
        if (await _fileStorageService.FileExistsAsync(blobPath, cancellationToken))
        {
            _logger.LogInformation("Image already exists for album {AlbumSku} at {BlobPath}", request.AlbumSku, blobPath);
            return ImageUploadResult.Success(blobPath, request.ImageUrl);
        }

        try
        {
            var imageBytes = await DownloadImageAsync(request.ImageUrl, cancellationToken);
            if (imageBytes == null || !IsValidImage(imageBytes))
            {
                _logger.LogWarning("Invalid image for album {AlbumSku} from {ImageUrl}", request.AlbumSku, request.ImageUrl);
                return ImageUploadResult.Failure("Invalid image format or empty content", request.ImageUrl);
            }

            using var imageStream = new MemoryStream(imageBytes);
            await _fileStorageService.UploadFileAsync(blobPath, imageStream, cancellationToken);

            _logger.LogInformation("Uploaded image for album {AlbumSku} to {BlobPath}", request.AlbumSku, blobPath);
            return ImageUploadResult.Success(blobPath, request.ImageUrl);
        }
        catch (FlurlHttpException httpException)
        {
            _logger.LogWarning("HTTP error {StatusCode} downloading image for album {AlbumSku} from {ImageUrl}", httpException.StatusCode, request.AlbumSku, request.ImageUrl);
            return ImageUploadResult.Failure($"HTTP error: {httpException.StatusCode}", request.ImageUrl);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to upload image for album {AlbumSku} from {ImageUrl}", request.AlbumSku, request.ImageUrl);
            return ImageUploadResult.Failure(exception.Message, request.ImageUrl);
        }
    }

    private async Task<byte[]?> DownloadImageAsync(string imageUrl, CancellationToken cancellationToken)
    {
        var userAgent = _userAgentProvider.GetRandomUserAgent();

        return await imageUrl
            .WithTimeout(TimeSpan.FromSeconds(_settings.RequestTimeoutSeconds))
            .WithHeader("User-Agent", userAgent)
            .WithHeader("Accept", "image/webp,image/apng,image/*,*/*;q=0.8")
            .WithHeader("Accept-Encoding", "gzip, deflate, br")
            .GetBytesAsync(cancellationToken: cancellationToken);
    }

    private bool IsValidImage(byte[] imageBytes)
    {
        if (imageBytes.Length < _settings.MinImageSizeBytes || imageBytes.Length > _settings.MaxImageSizeBytes)
            return false;

        try
        {
            using var stream = new MemoryStream(imageBytes);
            return stream.IsImage();
        }
        catch
        {
            return false;
        }
    }

    private string GenerateBlobPath(DistributorCode distributorCode, string albumSku, string originalUrl)
    {
        var extension = GetImageExtension(originalUrl);
        var sanitizedSku = SanitizeFileName(albumSku);

        return $"{_settings.ImageStorageFolder}/{distributorCode}/{sanitizedSku}{extension}";
    }

    private string GetImageExtension(string imageUrl)
    {
        try
        {
            var uri = new Uri(imageUrl);
            var extension = Path.GetExtension(uri.LocalPath).ToLowerInvariant();

            return _settings.SupportedExtensions.TryGetValue(extension, out var mappedExtension)
                ? mappedExtension
                : _settings.DefaultImageExtension;
        }
        catch
        {
            return _settings.DefaultImageExtension;
        }
    }

    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return _settings.DefaultFileName;

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());

        return string.IsNullOrEmpty(sanitized) ? _settings.DefaultFileName : sanitized;
    }
}