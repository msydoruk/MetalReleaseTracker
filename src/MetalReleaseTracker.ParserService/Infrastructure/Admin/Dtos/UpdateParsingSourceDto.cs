namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record UpdateParsingSourceDto(
    string ParsingUrl,
    bool IsEnabled);
