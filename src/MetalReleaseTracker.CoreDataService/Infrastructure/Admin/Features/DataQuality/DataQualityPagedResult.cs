namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality;

public class DataQualityPagedResult<T>
    where T : class
{
    public List<T> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
