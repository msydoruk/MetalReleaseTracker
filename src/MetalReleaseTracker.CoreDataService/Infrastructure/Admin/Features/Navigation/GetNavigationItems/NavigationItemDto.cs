namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;

public class NavigationItemDto
{
    public Guid Id { get; set; }

    public string Path { get; set; } = string.Empty;

    public string IconName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; }

    public bool IsProtected { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Dictionary<string, NavigationItemTranslationDto> Translations { get; set; } = new();
}
