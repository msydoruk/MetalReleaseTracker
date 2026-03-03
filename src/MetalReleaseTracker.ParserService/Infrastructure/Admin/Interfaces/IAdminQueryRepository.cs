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

    Task<PagedResultDto<CatalogueDetailDto>> GetCatalogueDetailsAsync(
        CatalogueDetailFilterDto filter,
        CancellationToken cancellationToken);

    Task<PagedResultDto<ParsingRunDto>> GetParsingRunsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<ParsingRunDto?> GetParsingRunByIdAsync(
        Guid runId,
        CancellationToken cancellationToken);

    Task<PagedResultDto<ParsingRunItemDto>> GetParsingRunItemsAsync(
        Guid runId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
