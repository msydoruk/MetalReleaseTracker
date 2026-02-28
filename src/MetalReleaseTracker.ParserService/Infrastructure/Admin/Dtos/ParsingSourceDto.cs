using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class ParsingSourceDto
{
    public Guid Id { get; set; }

    public DistributorCode DistributorCode { get; set; }

    public string Name { get; set; }

    public string ParsingUrl { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
