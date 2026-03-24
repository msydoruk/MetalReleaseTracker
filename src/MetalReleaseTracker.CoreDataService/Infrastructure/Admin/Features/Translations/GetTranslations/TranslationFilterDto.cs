namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.GetTranslations;

public class TranslationFilterDto
{
    public string? Category { get; set; }

    public string? Language { get; set; }

    public string? Search { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 50;
}
