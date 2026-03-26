namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;

public class AdminDistributorDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public int AlbumCount { get; set; }

    public bool IsVisible { get; set; }

    public string? Country { get; set; }

    public string? CountryFlag { get; set; }

    public string? LogoUrl { get; set; }

    public string? WebsiteUrl { get; set; }

    public Dictionary<string, DistributorTranslationDto> Translations { get; set; } = new();
}
