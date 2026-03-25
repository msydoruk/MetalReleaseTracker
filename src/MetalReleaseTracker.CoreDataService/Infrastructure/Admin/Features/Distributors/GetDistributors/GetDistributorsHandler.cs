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
        var distributors = await _context.Distributors
            .AsNoTracking()
            .Select(distributor => new AdminDistributorDto
            {
                Id = distributor.Id,
                Name = distributor.Name,
                Code = distributor.Code.ToString(),
                AlbumCount = _context.Albums.Count(album => album.DistributorId == distributor.Id),
                DescriptionEn = distributor.DescriptionEn,
                DescriptionUa = distributor.DescriptionUa,
                Country = distributor.Country,
                CountryFlag = distributor.CountryFlag,
                LogoUrl = distributor.LogoUrl,
                WebsiteUrl = distributor.WebsiteUrl,
            })
            .OrderBy(distributor => distributor.Name)
            .ToListAsync(cancellationToken);

        return distributors;
    }
}
