using AutoMapper;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;
using MetalReleaseTracker.SharedLibraries.Minio;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class UserFollowedBandService : IUserFollowedBandService
{
    private readonly IUserFollowedBandRepository _userFollowedBandRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public UserFollowedBandService(
        IUserFollowedBandRepository userFollowedBandRepository,
        IFileStorageService fileStorageService,
        IMapper mapper)
    {
        _userFollowedBandRepository = userFollowedBandRepository;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task FollowAsync(string userId, Guid bandId, CancellationToken cancellationToken = default)
    {
        var exists = await _userFollowedBandRepository.ExistsAsync(userId, bandId, cancellationToken);
        if (exists)
        {
            return;
        }

        var entity = new UserFollowedBandEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BandId = bandId,
            CreatedDate = DateTime.UtcNow,
        };

        await _userFollowedBandRepository.AddAsync(entity, cancellationToken);
    }

    public async Task UnfollowAsync(string userId, Guid bandId, CancellationToken cancellationToken = default)
    {
        await _userFollowedBandRepository.RemoveAsync(userId, bandId, cancellationToken);
    }

    public async Task<bool> IsFollowingAsync(string userId, Guid bandId, CancellationToken cancellationToken = default)
    {
        return await _userFollowedBandRepository.ExistsAsync(userId, bandId, cancellationToken);
    }

    public async Task<Dictionary<Guid, bool>> GetFollowedBandIdsAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _userFollowedBandRepository.GetFollowedBandIdsAsync(userId, cancellationToken);
    }

    public async Task<List<BandDto>> GetFollowedBandsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var entities = await _userFollowedBandRepository.GetFollowedBandsAsync(userId, cancellationToken);
        return entities.Select(follow => _mapper.Map<BandDto>(follow.Band)).ToList();
    }

    public async Task<int> GetFollowerCountAsync(Guid bandId, CancellationToken cancellationToken = default)
    {
        return await _userFollowedBandRepository.GetFollowerCountAsync(bandId, cancellationToken);
    }

    public async Task<PagedResultDto<AlbumDto>> GetFeedAsync(string userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var followedBandIds = await _userFollowedBandRepository.GetFollowedBandIdsAsync(userId, cancellationToken);
        var bandIds = followedBandIds.Keys.ToList();

        if (bandIds.Count == 0)
        {
            return new PagedResultDto<AlbumDto>
            {
                Items = [],
                TotalCount = 0,
                PageCount = 0,
                PageSize = pageSize,
                CurrentPage = page,
            };
        }

        var result = await _userFollowedBandRepository.GetFeedAlbumsAsync(bandIds, page, pageSize, cancellationToken);

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
            CurrentPage = result.CurrentPage,
        };
    }
}
