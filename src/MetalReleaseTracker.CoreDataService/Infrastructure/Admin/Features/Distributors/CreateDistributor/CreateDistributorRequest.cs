namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.CreateDistributor;

public class CreateDistributorRequest
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? DescriptionEn { get; set; }

    public string? DescriptionUa { get; set; }

    public string? Country { get; set; }

    public string? CountryFlag { get; set; }

    public string? LogoUrl { get; set; }

    public string? WebsiteUrl { get; set; }
}
