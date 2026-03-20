namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class AlbumSuggestionDto
{
    public string Text { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public Guid? Id { get; set; }
}
