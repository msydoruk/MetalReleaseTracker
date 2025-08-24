namespace MetalReleaseTracker.ParserService.Infrastructure.Images.Exeptions;

public class ImageUploadException : Exception
{
    public string? AlbumSku { get; }

    public string? ImageUrl { get; }

    public ImageUploadException(string albumSku, string imageUrl, string message)
        : base(message)
    {
        AlbumSku = albumSku;
        ImageUrl = imageUrl;
    }

    public ImageUploadException(string albumSku, string imageUrl, string message, Exception innerException)
        : base(message, innerException)
    {
        AlbumSku = albumSku;
        ImageUrl = imageUrl;
    }
}