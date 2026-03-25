using MetalReleaseTracker.CoreDataService.Configuration;
using MetalReleaseTracker.CoreDataService.Data;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.GetDistributors;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Distributors.CreateDistributor;

public class CreateDistributorHandler
{
    private readonly CoreDataServiceDbContext _context;

    public CreateDistributorHandler(CoreDataServiceDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDistributorDto?> HandleAsync(
        CreateDistributorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<DistributorCode>(request.Code, ignoreCase: true, out var distributorCode))
        {
            return null;
        }

        var entity = new DistributorEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Code = distributorCode,
            DescriptionEn = request.DescriptionEn,
            DescriptionUa = request.DescriptionUa,
            Country = request.Country,
            CountryFlag = request.CountryFlag,
            LogoUrl = request.LogoUrl,
            WebsiteUrl = request.WebsiteUrl,
        };

        _context.Distributors.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminDistributorDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code.ToString(),
            AlbumCount = 0,
            DescriptionEn = entity.DescriptionEn,
            DescriptionUa = entity.DescriptionUa,
            Country = entity.Country,
            CountryFlag = entity.CountryFlag,
            LogoUrl = entity.LogoUrl,
            WebsiteUrl = entity.WebsiteUrl,
        };
    }
}
