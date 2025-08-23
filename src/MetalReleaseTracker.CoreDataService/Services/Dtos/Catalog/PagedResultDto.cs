namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class PagedResultDto<T> where T : class
{
    public List<T> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int PageCount { get; set; }

    public int PageSize { get; set; }

    public int CurrentPage { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;

    public bool HasNextPage => CurrentPage < PageCount;
}