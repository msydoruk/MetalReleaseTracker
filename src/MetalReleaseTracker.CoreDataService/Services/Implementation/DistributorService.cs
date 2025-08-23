using AutoMapper;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class DistributorService : IDistributorService
{
    private readonly IDistributorsRepository _distributorsRepository;
    private readonly IMapper _mapper;

    public DistributorService(IDistributorsRepository distributorsRepository, IMapper mapper)
    {
        _distributorsRepository = distributorsRepository;
        _mapper = mapper;
    }

    public async Task<List<DistributorDto>> GetAllDistributorsAsync(CancellationToken cancellationToken = default)
    {
        var distributors = await _distributorsRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<DistributorDto>>(distributors);
    }

    public async Task<DistributorDto?> GetDistributorByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var distributor = await _distributorsRepository.GetByIdAsync(id, cancellationToken);
        return distributor == null ? null : _mapper.Map<DistributorDto>(distributor);
    }

    public async Task<List<DistributorWithAlbumCountDto>> GetDistributorsWithAlbumCountAsync(CancellationToken cancellationToken = default)
    {
        return await _distributorsRepository.GetDistributorsWithAlbumCountAsync(cancellationToken);
    }
}