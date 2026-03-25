namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBandById;

public class AdminBandDetailDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Genre { get; set; }

    public string? PhotoUrl { get; set; }

    public string? MetalArchivesUrl { get; set; }

    public int? FormationYear { get; set; }

    public int AlbumCount { get; set; }

    public string Slug { get; set; } = string.Empty;

    public List<AdminBandAlbumDto> Albums { get; set; } = [];

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }
}
