namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class AlbumParsedEventDto
{
    public Guid Id { get; set; }

    public Guid ParsingSessionId { get; set; }

    public DateTime CreatedDate { get; set; }

    public string EventPayloadPreview { get; set; } = string.Empty;
}
