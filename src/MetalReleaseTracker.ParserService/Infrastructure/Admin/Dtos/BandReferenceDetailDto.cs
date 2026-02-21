namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class BandReferenceDetailDto
{
    public Guid Id { get; set; }

    public string BandName { get; set; } = string.Empty;

    public long MetalArchivesId { get; set; }

    public string Genre { get; set; } = string.Empty;

    public DateTime LastSyncedAt { get; set; }

    public int DiscographyCount { get; set; }

    public List<BandDiscographyDto> Discography { get; set; } = [];
}
