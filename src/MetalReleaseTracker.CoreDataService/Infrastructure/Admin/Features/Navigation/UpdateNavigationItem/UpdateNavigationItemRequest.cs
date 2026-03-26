using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.GetNavigationItems;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Navigation.UpdateNavigationItem;

public class UpdateNavigationItemRequest
{
    public string? Path { get; set; }

    public string? IconName { get; set; }

    public int? SortOrder { get; set; }

    public bool? IsVisible { get; set; }

    public bool? IsProtected { get; set; }

    public Dictionary<string, NavigationItemTranslationDto>? Translations { get; set; }
}
