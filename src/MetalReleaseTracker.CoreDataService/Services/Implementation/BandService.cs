using AutoMapper;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class BandService : IBandService
{
    private readonly IBandRepository _bandRepository;
    private readonly IMapper _mapper;

    public BandService(IBandRepository bandRepository, IMapper mapper)
    {
        _bandRepository = bandRepository;
        _mapper = mapper;
    }

    public async Task<List<BandDto>> GetAllBandsAsync(CancellationToken cancellationToken = default)
    {
        var bands = await _bandRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<BandDto>>(bands);
    }

    public async Task<BandDto?> GetBandByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var band = await _bandRepository.GetByIdAsync(id, cancellationToken);
        return _mapper.Map<BandDto>(band);
    }

    public async Task<List<BandWithAlbumCountDto>> GetBandsWithAlbumCountAsync(CancellationToken cancellationToken = default)
    {
        return await _bandRepository.GetBandsWithAlbumCountAsync(cancellationToken);
    }
}