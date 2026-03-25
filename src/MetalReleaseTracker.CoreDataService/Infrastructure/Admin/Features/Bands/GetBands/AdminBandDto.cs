namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;

public class AdminBandDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Genre { get; set; }

    public string? PhotoUrl { get; set; }

    public string? MetalArchivesUrl { get; set; }

    public int? FormationYear { get; set; }

    public int AlbumCount { get; set; }

    public string Slug { get; set; } = string.Empty;

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }
}
