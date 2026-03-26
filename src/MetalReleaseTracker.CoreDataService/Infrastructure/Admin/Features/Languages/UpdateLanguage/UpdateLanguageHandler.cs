using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.GetLanguages;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.UpdateLanguage;

public class UpdateLanguageHandler
{
    private readonly CoreDataServiceDbContext _context;

    public UpdateLanguageHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<(LanguageDto? Result, string? Error)> HandleAsync(
        string code,
        UpdateLanguageRequest request,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.Languages
            .FirstOrDefaultAsync(language => language.Code == code, cancellationToken);

        if (entity is null)
        {
            return (null, "not_found");
        }

        if (request.IsDefault && !entity.IsDefault)
        {
            var currentDefault = await _context.Languages
                .FirstOrDefaultAsync(language => language.IsDefault && language.Code != code, cancellationToken);

            if (currentDefault is not null)
            {
                currentDefault.IsDefault = false;
            }
        }

        if (!request.IsDefault && entity.IsDefault)
        {
            return (null, "Cannot remove default status. Set another language as default first.");
        }

        entity.Name = request.Name;
        entity.NativeName = request.NativeName;
        entity.SortOrder = request.SortOrder;
        entity.IsEnabled = request.IsEnabled;
        entity.IsDefault = request.IsDefault;

        await _context.SaveChangesAsync(cancellationToken);

        return (new LanguageDto
        {
            Code = entity.Code,
            Name = entity.Name,
            NativeName = entity.NativeName,
            SortOrder = entity.SortOrder,
            IsEnabled = entity.IsEnabled,
            IsDefault = entity.IsDefault,
            CreatedAt = entity.CreatedAt,
        }, null);
    }
}
