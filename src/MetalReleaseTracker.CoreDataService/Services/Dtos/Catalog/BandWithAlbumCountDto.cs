namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class BandWithAlbumCountDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Genre { get; set; }

    public int AlbumCount { get; set; }
}