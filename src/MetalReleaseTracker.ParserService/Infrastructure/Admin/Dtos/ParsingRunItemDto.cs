namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class ParsingRunItemDto
{
    public Guid Id { get; set; }

    public string ItemDescription { get; set; } = string.Empty;

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public string[] Categories { get; set; } = [];

    public DateTime ProcessedAt { get; set; }
}
