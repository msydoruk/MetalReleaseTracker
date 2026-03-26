namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.CreateLanguage;

public class CreateLanguageRequest
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string NativeName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; } = true;
}
