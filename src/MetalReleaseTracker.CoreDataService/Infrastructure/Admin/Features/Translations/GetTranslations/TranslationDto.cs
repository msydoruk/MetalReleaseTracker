namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.GetTranslations;

public class TranslationDto
{
    public Guid Id { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Language { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }
}
