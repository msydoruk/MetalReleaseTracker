namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.UpdateDistributor;

public class UpdateDistributorRequest
{
    public string Name { get; set; } = string.Empty;

    public string? DescriptionEn { get; set; }

    public string? DescriptionUa { get; set; }

    public string? Country { get; set; }

    public string? CountryFlag { get; set; }

    public string? LogoUrl { get; set; }

    public string? WebsiteUrl { get; set; }
}
