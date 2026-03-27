namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetPotentialDuplicateBands;

public class DuplicateBandGroupDto
{
    public string NormalizedName { get; set; } = string.Empty;

    public List<DuplicateBandDto> Bands { get; set; } = [];
}

public class DuplicateBandDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Genre { get; set; }

    public int AlbumCount { get; set; }

    public bool IsVisible { get; set; }
}
