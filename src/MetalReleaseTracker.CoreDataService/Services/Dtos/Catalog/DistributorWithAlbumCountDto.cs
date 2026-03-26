namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class DistributorWithAlbumCountDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int AlbumCount { get; set; }

    public string? Description { get; set; }

    public string? Country { get; set; }

    public string? CountryFlag { get; set; }

    public string? LogoUrl { get; set; }

    public string? WebsiteUrl { get; set; }
}
