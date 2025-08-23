using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Domain.Models.Entities;

public class ParserDataSource
{
    public DistributorCode DistributorCode { get; set; }

    public string Name { get; set; }

    public string ParsingUrl { get; set; }
}