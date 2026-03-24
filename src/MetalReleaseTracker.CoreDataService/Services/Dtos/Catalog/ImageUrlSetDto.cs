namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class ImageUrlSetDto
{
    public string Original { get; set; } = string.Empty;

    public string? Small { get; set; }

    public string? Medium { get; set; }

    public string? Large { get; set; }
}
