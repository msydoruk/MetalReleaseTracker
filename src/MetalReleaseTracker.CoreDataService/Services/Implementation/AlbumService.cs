using AutoMapper;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class AlbumService : IAlbumService
{
    private readonly IAlbumRepository _albumRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public AlbumService(IAlbumRepository albumRepository, IFileStorageService fileStorageService, IMapper mapper)
    {
        _albumRepository = albumRepository;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<AlbumDto>> GetFilteredAlbums(AlbumFilterDto filter, CancellationToken cancellationToken = default)
    {
        var result = await _albumRepository.GetFilteredAlbumsAsync(filter, cancellationToken);

        var albumDtos = new List<AlbumDto>();
        foreach (var album in result.Items)
        {
            var albumDto = _mapper.Map<AlbumDto>(album);
            albumDto.PhotoUrl = await _fileStorageService.GetFileUrlAsync(album.PhotoUrl, cancellationToken);
            albumDtos.Add(albumDto);
        }

        var pagedDtos = new PagedResultDto<AlbumDto>
        {
            Items = albumDtos,
            TotalCount = result.TotalCount,
            PageCount = result.PageCount,
            PageSize = result.PageSize,
            CurrentPage = result.CurrentPage
        };

        return pagedDtos;
    }

    public async Task<AlbumDto?> GetAlbumById(Guid id, CancellationToken cancellationToken = default)
    {
        var album = await _albumRepository.GetAsync(id, cancellationToken);
        return album == null ? null : _mapper.Map<AlbumDto>(album);
    }
}