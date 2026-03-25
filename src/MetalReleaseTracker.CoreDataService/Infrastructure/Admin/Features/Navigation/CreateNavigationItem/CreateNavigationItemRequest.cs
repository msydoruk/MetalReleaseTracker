namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.CreateNavigationItem;

public class CreateNavigationItemRequest
{
    public string TitleEn { get; set; } = string.Empty;

    public string TitleUa { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public string IconName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; }

    public bool IsProtected { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? SeoKeywords { get; set; }
}
