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
            Country = request.Country,
            CountryFlag = request.CountryFlag,
            LogoUrl = request.LogoUrl,
            WebsiteUrl = request.WebsiteUrl,
        };

        foreach (var (languageCode, translationDto) in request.Translations)
        {
            entity.Translations.Add(new DistributorTranslationEntity
            {
                Id = Guid.NewGuid(),
                DistributorId = entity.Id,
                LanguageCode = languageCode,
                Description = translationDto.Description,
            });
        }

        _context.Distributors.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return new AdminDistributorDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Code = entity.Code.ToString(),
            AlbumCount = 0,
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
        };
    }
}
