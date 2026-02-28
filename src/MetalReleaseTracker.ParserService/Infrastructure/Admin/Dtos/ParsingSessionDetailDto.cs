using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class ParsingSessionDetailDto
{
    public Guid Id { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    public AlbumParsingStatus ParsingStatus { get; set; }

    public int EventCount { get; set; }

    public List<AlbumParsedEventDto> Events { get; set; } = [];
}
