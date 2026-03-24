namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;

public class AdminBandPagedResult
{
    public List<AdminBandDto> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
