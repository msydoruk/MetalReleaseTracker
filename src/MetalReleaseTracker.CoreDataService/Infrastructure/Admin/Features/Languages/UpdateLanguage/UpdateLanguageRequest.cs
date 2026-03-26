namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.UpdateLanguage;

public class UpdateLanguageRequest
{
    public string Name { get; set; } = string.Empty;

    public string NativeName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsDefault { get; set; }
}
