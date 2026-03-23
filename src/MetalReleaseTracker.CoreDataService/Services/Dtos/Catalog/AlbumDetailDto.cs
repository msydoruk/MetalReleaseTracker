using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class AlbumDetailDto
{
    public Guid PrimaryAlbumId { get; set; }

    public string AlbumName { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public string? Genre { get; set; }

    public AlbumMediaType? Media { get; set; }

    public AlbumStatus? Status { get; set; }

    public string? CanonicalTitle { get; set; }

    public int? OriginalYear { get; set; }

    public string? Description { get; set; }

    public string? Label { get; set; }

    public string? Press { get; set; }

    public Guid BandId { get; set; }

    public string BandName { get; set; } = string.Empty;

    public string? BandPhotoUrl { get; set; }

    public string? BandGenre { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string BandSlug { get; set; } = string.Empty;

    public List<AlbumVariantDto> Variants { get; set; } = [];

    public List<RelatedAlbumDto> RelatedReleases { get; set; } = [];
}
