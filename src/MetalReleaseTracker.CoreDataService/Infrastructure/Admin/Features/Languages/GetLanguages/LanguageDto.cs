namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.GetLanguages;

public class LanguageDto
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string NativeName { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; }
}
