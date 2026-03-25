using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;
using Microsoft.EntityFrameworkCore;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributorById;

public class GetDistributorByIdHandler
{
    private readonly CoreDataServiceDbContext _context;

    public GetDistributorByIdHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDistributorDto?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var distributor = await _context.Distributors
            .AsNoTracking()
            .Where(distributor => distributor.Id == id)
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
            .FirstOrDefaultAsync(cancellationToken);

        return distributor;
    }
}
