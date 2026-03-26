using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Languages.GetLanguages;

public class GetLanguagesHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetLanguagesHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<LanguageDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Languages
            .AsNoTracking()
            .OrderBy(language => language.SortOrder)
            .Select(language => new LanguageDto
            {
                Code = language.Code,
                Name = language.Name,
                NativeName = language.NativeName,
                SortOrder = language.SortOrder,
                IsEnabled = language.IsEnabled,
                IsDefault = language.IsDefault,
                CreatedAt = language.CreatedAt,
            })
            .ToListAsync(cancellationToken);
    }
}
