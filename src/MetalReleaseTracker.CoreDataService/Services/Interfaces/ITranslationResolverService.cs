namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface ITranslationResolverService
{
    T? Resolve<T>(
        IEnumerable<T> translations,
        string languageCode,
        Func<T, string> languageCodeSelector,
        string defaultLanguageCode = "en")
        where T : class;

    string? ResolveField<T>(
        IEnumerable<T> translations,
        string languageCode,
        Func<T, string> languageCodeSelector,
        Func<T, string?> fieldSelector,
        string defaultLanguageCode = "en")
        where T : class;
}
