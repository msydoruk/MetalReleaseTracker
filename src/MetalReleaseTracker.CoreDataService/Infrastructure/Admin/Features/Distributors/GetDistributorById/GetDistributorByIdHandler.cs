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
        var entity = await _context.Distributors
            .AsNoTracking()
            .FirstOrDefaultAsync(distributor => distributor.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var albumCount = await _context.Albums
            .CountAsync(album => album.DistributorId == entity.Id, cancellationToken);

        return new AdminDistributorDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code.ToString(),
            AlbumCount = albumCount,
            IsVisible = entity.IsVisible,
            DescriptionEn = entity.DescriptionEn,
            DescriptionUa = entity.DescriptionUa,
            Country = entity.Country,
            CountryFlag = entity.CountryFlag,
            LogoUrl = entity.LogoUrl,
            WebsiteUrl = entity.WebsiteUrl,
        };
    }
}
