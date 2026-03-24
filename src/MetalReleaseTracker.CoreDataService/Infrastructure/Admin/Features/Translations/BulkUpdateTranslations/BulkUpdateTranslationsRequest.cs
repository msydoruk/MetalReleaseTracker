namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.BulkUpdateTranslations;

public class BulkUpdateTranslationsRequest
{
    public List<TranslationUpdateItem> Updates { get; set; } = [];
}

public class TranslationUpdateItem
{
    public Guid Id { get; set; }

    public string Value { get; set; } = string.Empty;
}
