using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class ParsingSessionDto
{
    public Guid Id { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public DateTime LastUpdatedDate { get; set; }

    public AlbumParsingStatus ParsingStatus { get; set; }

    public int EventCount { get; set; }
}
