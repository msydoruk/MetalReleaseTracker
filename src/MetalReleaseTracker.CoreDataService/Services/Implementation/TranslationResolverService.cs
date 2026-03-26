using MetalReleaseTracker.CoreDataService.Services.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class TranslationResolverService : ITranslationResolverService
{
    public T? Resolve<T>(
        IEnumerable<T> translations,
        string languageCode,
        Func<T, string> languageCodeSelector,
        string defaultLanguageCode = "en")
        where T : class
    {
        var list = translations.ToList();
        return list.FirstOrDefault(translation => languageCodeSelector(translation) == languageCode)
            ?? list.FirstOrDefault(translation => languageCodeSelector(translation) == defaultLanguageCode);
    }

    public string? ResolveField<T>(
        IEnumerable<T> translations,
        string languageCode,
        Func<T, string> languageCodeSelector,
        Func<T, string?> fieldSelector,
        string defaultLanguageCode = "en")
        where T : class
    {
        var resolved = Resolve(translations, languageCode, languageCodeSelector, defaultLanguageCode);
        return resolved is not null ? fieldSelector(resolved) : null;
    }
}
