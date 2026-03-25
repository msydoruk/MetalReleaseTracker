namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Seo;

public class SeoMetaTagsDto
{
    public string Title { get; set; } = "Metal Release Tracker";

    public string Description { get; set; } = string.Empty;

    public string CanonicalUrl { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public string OgType { get; set; } = "website";

    public string? Keywords { get; set; }
}
