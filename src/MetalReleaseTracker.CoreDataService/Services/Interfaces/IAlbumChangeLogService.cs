using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

namespace MetalReleaseTracker.CoreDataService.Services.Interfaces;

public interface IAlbumChangeLogService
{
    Task<PagedResultDto<AlbumChangeLogDto>> GetChangeLogAsync(ChangeLogFilterDto filter, CancellationToken cancellationToken = default);

    Task<List<PriceHistoryPointDto>> GetPriceHistoryAsync(string albumName, string bandName, CancellationToken cancellationToken = default);
}
