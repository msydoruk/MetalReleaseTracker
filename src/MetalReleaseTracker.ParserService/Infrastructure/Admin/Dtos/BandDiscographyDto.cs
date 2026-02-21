namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class BandDiscographyDto
{
    public Guid Id { get; set; }

    public string AlbumTitle { get; set; } = string.Empty;

    public string AlbumType { get; set; } = string.Empty;

    public int? Year { get; set; }
}
