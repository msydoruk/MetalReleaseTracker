namespace MetalReleaseTracker.ParserService.Infrastructure.Images.Models;

public class ImageUploadResult
{
    public bool IsSuccess { get; set; }

    public string? BlobPath { get; set; }

    public string? ErrorMessage { get; set; }

    public string? OriginalUrl { get; set; }

    public static ImageUploadResult Success(string blobPath, string originalUrl) => new()
    {
        IsSuccess = true,
        BlobPath = blobPath,
        OriginalUrl = originalUrl
    };

    public static ImageUploadResult Failure(string errorMessage, string originalUrl) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        OriginalUrl = originalUrl
    };
}