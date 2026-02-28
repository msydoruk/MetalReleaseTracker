using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;

public interface IAdminQueryRepository
{
    Task<PagedResultDto<BandReferenceDto>> GetBandReferencesAsync(
        BandReferenceFilterDto filter,
        CancellationToken cancellationToken);

    Task<BandReferenceDetailDto?> GetBandReferenceByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<PagedResultDto<CatalogueIndexDto>> GetCatalogueIndexAsync(
        CatalogueIndexFilterDto filter,
        CancellationToken cancellationToken);

    Task<PagedResultDto<ParsingSessionDto>> GetParsingSessionsAsync(
        ParsingSessionFilterDto filter,
        CancellationToken cancellationToken);

    Task<ParsingSessionDetailDto?> GetParsingSessionByIdAsync(
        Guid id,
        CancellationToken cancellationToken);
}
