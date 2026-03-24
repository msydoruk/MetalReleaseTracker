namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;

public class AdminAlbumFilterDto
{
    public string? Search { get; set; }

    public Guid? BandId { get; set; }

    public Guid? DistributorId { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
