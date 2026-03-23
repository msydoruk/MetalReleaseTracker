using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class RelatedAlbumDto
{
    public Guid AlbumId { get; set; }

    public string AlbumName { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public AlbumMediaType? Media { get; set; }

    public int? OriginalYear { get; set; }

    public float MinPrice { get; set; }

    public string AlbumSlug { get; set; } = string.Empty;
}
