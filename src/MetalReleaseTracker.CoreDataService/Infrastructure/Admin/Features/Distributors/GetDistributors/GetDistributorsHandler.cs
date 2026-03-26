using MetalReleaseTracker.CoreDataService.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;

public class GetDistributorsHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetDistributorsHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminDistributorDto>> HandleAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.Distributors
            .AsNoTracking()
            .Include(distributor => distributor.Translations)
            .OrderBy(distributor => distributor.Name)
            .ToListAsync(cancellationToken);

        var distributors = new List<AdminDistributorDto>();
        foreach (var entity in entities)
        {
            var albumCount = await _context.Albums
                .CountAsync(album => album.DistributorId == entity.Id, cancellationToken);

            distributors.Add(new AdminDistributorDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Code = entity.Code.ToString(),
                AlbumCount = albumCount,
                IsVisible = entity.IsVisible,
                Country = entity.Country,
                CountryFlag = entity.CountryFlag,
                LogoUrl = entity.LogoUrl,
                WebsiteUrl = entity.WebsiteUrl,
                Translations = entity.Translations.ToDictionary(
                    translation => translation.LanguageCode,
                    translation => new DistributorTranslationDto
                    {
                        Description = translation.Description,
                    }),
            });
        }

        return distributors;
    }
}
