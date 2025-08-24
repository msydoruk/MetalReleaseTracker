using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Images.Models;

public class ImageUploadRequest
{
    public string ImageUrl { get; set; }

    public string AlbumSku { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public string? AlbumName { get; set; }

    public string? BandName { get; set; }
}