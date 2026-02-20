using AutoMapper;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class BandService : IBandService
{
    private readonly IBandRepository _bandRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public BandService(IBandRepository bandRepository, IFileStorageService fileStorageService, IMapper mapper)
    {
        _bandRepository = bandRepository;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task<List<BandDto>> GetAllBandsAsync(CancellationToken cancellationToken = default)
    {
        var bands = await _bandRepository.GetAllAsync(cancellationToken);
        var bandDtos = _mapper.Map<List<BandDto>>(bands);

        foreach (var band in bandDtos.Where(band => !string.IsNullOrEmpty(band.PhotoUrl)))
        {
            band.PhotoUrl = await _fileStorageService.GetFileUrlAsync(band.PhotoUrl!, cancellationToken);
        }

        return bandDtos;
    }

    public async Task<BandDto?> GetBandByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var band = await _bandRepository.GetByIdAsync(id, cancellationToken);
        var bandDto = _mapper.Map<BandDto>(band);

        if (bandDto != null && !string.IsNullOrEmpty(bandDto.PhotoUrl))
        {
            bandDto.PhotoUrl = await _fileStorageService.GetFileUrlAsync(bandDto.PhotoUrl, cancellationToken);
        }

        return bandDto;
    }

    public async Task<List<BandWithAlbumCountDto>> GetBandsWithAlbumCountAsync(CancellationToken cancellationToken = default)
    {
        var bands = await _bandRepository.GetBandsWithAlbumCountAsync(cancellationToken);

        foreach (var band in bands.Where(band => !string.IsNullOrEmpty(band.PhotoUrl)))
        {
            band.PhotoUrl = await _fileStorageService.GetFileUrlAsync(band.PhotoUrl!, cancellationToken);
        }

        return bands;
    }
}