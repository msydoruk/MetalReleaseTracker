using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.BulkUpdateTranslations;

public class BulkUpdateTranslationsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public BulkUpdateTranslationsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<int> HandleAsync(
        BulkUpdateTranslationsRequest request,
        CancellationToken cancellationToken = default)
    {
        var updateIds = request.Updates.Select(update => update.Id).ToList();
        var updateMap = request.Updates.ToDictionary(update => update.Id, update => update.Value);

        var translations = await _context.Translations
            .Where(translation => updateIds.Contains(translation.Id))
            .ToListAsync(cancellationToken);

        foreach (var translation in translations)
        {
            if (updateMap.TryGetValue(translation.Id, out var newValue))
            {
                translation.Value = newValue;
                translation.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return translations.Count;
    }
}
