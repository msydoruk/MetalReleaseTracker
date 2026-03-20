using AutoMapper;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class AlbumChangeLogService : IAlbumChangeLogService
{
    private readonly IAlbumChangeLogRepository _albumChangeLogRepository;
    private readonly IMapper _mapper;

    public AlbumChangeLogService(IAlbumChangeLogRepository albumChangeLogRepository, IMapper mapper)
    {
        _albumChangeLogRepository = albumChangeLogRepository;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<AlbumChangeLogDto>> GetChangeLogAsync(ChangeLogFilterDto filter, CancellationToken cancellationToken = default)
    {
        var pagedResult = await _albumChangeLogRepository.GetPagedAsync(filter.Page, filter.PageSize, cancellationToken);

        return new PagedResultDto<AlbumChangeLogDto>
        {
            Items = _mapper.Map<List<AlbumChangeLogDto>>(pagedResult.Items),
            TotalCount = pagedResult.TotalCount,
            PageCount = pagedResult.PageCount,
            PageSize = pagedResult.PageSize,
            CurrentPage = pagedResult.CurrentPage,
        };
    }

    public async Task<List<PriceHistoryPointDto>> GetPriceHistoryAsync(string albumName, string bandName, CancellationToken cancellationToken = default)
    {
        var entities = await _albumChangeLogRepository.GetByAlbumNameAsync(albumName, bandName, cancellationToken);

        return entities.Select(entity => new PriceHistoryPointDto
        {
            Price = entity.Price,
            OldPrice = entity.OldPrice,
            ChangeType = entity.ChangeType,
            ChangedAt = entity.ChangedAt,
            DistributorName = entity.DistributorName,
        }).ToList();
    }
}
