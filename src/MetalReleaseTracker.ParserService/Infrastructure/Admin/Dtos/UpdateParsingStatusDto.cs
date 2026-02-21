using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record UpdateParsingStatusDto(AlbumParsingStatus ParsingStatus);
