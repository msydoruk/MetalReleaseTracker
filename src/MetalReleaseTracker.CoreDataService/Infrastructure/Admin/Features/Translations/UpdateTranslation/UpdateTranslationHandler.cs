using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.GetTranslations;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Translations.UpdateTranslation;

public class UpdateTranslationHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateTranslationHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<TranslationDto?> HandleAsync(
        Guid id,
        UpdateTranslationRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Translations
            .FirstOrDefaultAsync(
                translation => translation.Id == id,
                cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Value = request.Value;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new TranslationDto
        {
            Id = entity.Id,
            Key = entity.Key,
            Language = entity.Language,
            Value = entity.Value,
            Category = entity.Category,
            UpdatedAt = entity.UpdatedAt,
        };
    }
}
