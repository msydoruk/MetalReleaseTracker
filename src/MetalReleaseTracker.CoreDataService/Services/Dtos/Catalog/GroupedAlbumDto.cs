using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class GroupedAlbumDto
{
    public string BandName { get; set; } = string.Empty;

    public string AlbumName { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public DateTime? ReleaseDate { get; set; }

    public string? Genre { get; set; }

    public AlbumMediaType? Media { get; set; }

    public AlbumStatus? Status { get; set; }

    public string? CanonicalTitle { get; set; }

    public int? OriginalYear { get; set; }

    public List<AlbumVariantDto> Variants { get; set; } = [];
}
