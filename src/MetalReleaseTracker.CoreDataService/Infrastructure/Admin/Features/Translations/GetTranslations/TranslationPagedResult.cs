namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.GetTranslations;

public class TranslationPagedResult
{
    public List<TranslationDto> Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
