namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;

public class AdminBandFilterDto
{
    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
