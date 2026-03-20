using AutoMapper;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class UserFavoriteService : IUserFavoriteService
{
    private readonly IUserFavoriteRepository _userFavoriteRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public UserFavoriteService(
        IUserFavoriteRepository userFavoriteRepository,
        IFileStorageService fileStorageService,
        IMapper mapper)
    {
        _userFavoriteRepository = userFavoriteRepository;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task AddFavoriteAsync(string userId, Guid albumId, UserCollectionStatus status = UserCollectionStatus.Favorite, CancellationToken cancellationToken = default)
    {
        var exists = await _userFavoriteRepository.ExistsAsync(userId, albumId, cancellationToken);
        if (exists)
        {
            await _userFavoriteRepository.UpdateStatusAsync(userId, albumId, status, cancellationToken);
            return;
        }

        var entity = new UserFavoriteEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AlbumId = albumId,
            CreatedDate = DateTime.UtcNow,
            Status = status
        };

        await _userFavoriteRepository.AddAsync(entity, cancellationToken);
    }

    public async Task RemoveFavoriteAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        await _userFavoriteRepository.RemoveAsync(userId, albumId, cancellationToken);
    }

    public async Task<bool> IsFavoriteAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        return await _userFavoriteRepository.ExistsAsync(userId, albumId, cancellationToken);
    }

    public async Task<Dictionary<Guid, int>> GetFavoriteIdsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var favorites = await _userFavoriteRepository.GetFavoriteAlbumIdsAsync(userId, cancellationToken);
        return favorites.ToDictionary(pair => pair.Key, pair => (int)pair.Value);
    }

    public async Task<PagedResultDto<AlbumDto>> GetFavoriteAlbumsAsync(string userId, int page, int pageSize, UserCollectionStatus? status = null, CancellationToken cancellationToken = default)
    {
        var result = await _userFavoriteRepository.GetFavoriteAlbumsAsync(userId, page, pageSize, status, cancellationToken);

        var albumDtos = new List<AlbumDto>();
        foreach (var album in result.Items)
        {
            var albumDto = _mapper.Map<AlbumDto>(album);
            albumDto.PhotoUrl = await _fileStorageService.GetFileUrlAsync(album.PhotoUrl, cancellationToken);
            albumDtos.Add(albumDto);
        }

        return new PagedResultDto<AlbumDto>
        {
            Items = albumDtos,
            TotalCount = result.TotalCount,
            PageCount = result.PageCount,
            PageSize = result.PageSize,
            CurrentPage = result.CurrentPage
        };
    }

    public async Task UpdateStatusAsync(string userId, Guid albumId, UserCollectionStatus status, CancellationToken cancellationToken = default)
    {
        await _userFavoriteRepository.UpdateStatusAsync(userId, albumId, status, cancellationToken);
    }
}
