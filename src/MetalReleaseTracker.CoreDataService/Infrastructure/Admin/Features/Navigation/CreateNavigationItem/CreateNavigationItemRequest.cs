using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.CreateNavigationItem;

public class CreateNavigationItemRequest
{
    public string Path { get; set; } = string.Empty;

    public string IconName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsVisible { get; set; }

    public bool IsProtected { get; set; }

    public Dictionary<string, NavigationItemTranslationDto> Translations { get; set; } = new();
}
