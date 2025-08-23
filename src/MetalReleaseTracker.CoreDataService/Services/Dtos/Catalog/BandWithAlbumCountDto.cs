namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class BandWithAlbumCountDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int AlbumCount { get; set; }
}