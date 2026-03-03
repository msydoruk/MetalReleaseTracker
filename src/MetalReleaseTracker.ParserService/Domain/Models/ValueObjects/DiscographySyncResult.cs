namespace MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

public class DiscographySyncResult
{
    public List<string> NewAlbumTitles { get; set; } = [];

    public int TotalCount { get; set; }
}
