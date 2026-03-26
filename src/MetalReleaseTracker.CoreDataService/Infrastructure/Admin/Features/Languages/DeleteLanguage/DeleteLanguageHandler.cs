using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.DeleteLanguage;

public class DeleteLanguageHandler
{
    private readonly CoreDataServiceDbContext _context;

    public DeleteLanguageHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<string?> HandleAsync(string code, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Languages
            .FirstOrDefaultAsync(language => language.Code == code, cancellationToken);

        if (entity is null)
        {
            return "not_found";
        }

        if (entity.IsDefault)
        {
            return "Cannot delete the default language.";
        }

        var hasTranslations = await _context.Translations
            .AnyAsync(translation => translation.Language == code, cancellationToken);

        if (hasTranslations)
        {
            return "Cannot delete a language that has translations. Remove all translations for this language first.";
        }

        _context.Languages.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return null;
    }
}
