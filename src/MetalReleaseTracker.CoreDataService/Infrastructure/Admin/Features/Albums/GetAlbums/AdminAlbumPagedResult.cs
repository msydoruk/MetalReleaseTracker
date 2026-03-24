namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;

public class AdminAlbumPagedResult
{
    public List<AdminAlbumDto> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
