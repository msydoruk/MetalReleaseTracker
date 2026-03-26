using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.GetLanguages;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.CreateLanguage;

public class CreateLanguageHandler
{
    private readonly CoreDataServiceDbContext _context;

    public CreateLanguageHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<LanguageDto?> HandleAsync(
        CreateLanguageRequest request,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Languages
            .AnyAsync(language => language.Code == request.Code, cancellationToken);

        if (exists)
        {
            return null;
        }

        var entity = new LanguageEntity
        {
            Code = request.Code.ToLowerInvariant(),
            Name = request.Name,
            NativeName = request.NativeName,
            SortOrder = request.SortOrder,
            IsEnabled = request.IsEnabled,
            IsDefault = false,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Languages.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new LanguageDto
        {
            Code = entity.Code,
            Name = entity.Name,
            NativeName = entity.NativeName,
            SortOrder = entity.SortOrder,
            IsEnabled = entity.IsEnabled,
            IsDefault = entity.IsDefault,
            CreatedAt = entity.CreatedAt,
        };
    }
}
