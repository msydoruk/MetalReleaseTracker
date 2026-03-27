namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetBandsMissingGenre;

public class DataQualityBandDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Genre { get; set; }

    public string? PhotoUrl { get; set; }

    public bool IsVisible { get; set; }

    public int AlbumCount { get; set; }
}
