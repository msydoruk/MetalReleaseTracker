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

    public async Task<PagedResultDto<GroupedAlbumDto>> GetGroupedAlbums(AlbumFilterDto filter, CancellationToken cancellationToken = default)
    {
        var allAlbums = await _albumRepository.GetAllFilteredAlbumsAsync(filter, cancellationToken);

        var groups = new List<List<Data.Entities.AlbumEntity>>();
        var assigned = new HashSet<int>();

        for (var i = 0; i < allAlbums.Count; i++)
        {
            if (assigned.Contains(i))
            {
                continue;
            }

            var group = new List<Data.Entities.AlbumEntity> { allAlbums[i] };
            assigned.Add(i);

            for (var j = i + 1; j < allAlbums.Count; j++)
            {
                if (assigned.Contains(j))
                {
                    continue;
                }

                if (AreAlbumsMatching(allAlbums[i], allAlbums[j]))
                {
                    group.Add(allAlbums[j]);
                    assigned.Add(j);
                }
            }

            groups.Add(group);
        }

        var totalCount = groups.Count;
        var pageSize = filter.PageSize;
        var page = filter.Page;
        var pageCount = (int)Math.Ceiling((double)totalCount / pageSize);

        var pagedGroups = groups
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var groupedDtos = new List<GroupedAlbumDto>();
        foreach (var group in pagedGroups)
        {
            var primary = group[0];
            var photoUrl = await _fileStorageService.GetFileUrlAsync(primary.PhotoUrl, cancellationToken);

            var variants = group.Select(album => new AlbumVariantDto
            {
                AlbumId = album.Id,
                DistributorId = album.DistributorId,
                DistributorName = album.Distributor?.Name ?? string.Empty,
                Price = album.Price,
                PurchaseUrl = album.PurchaseUrl
            }).ToList();

            groupedDtos.Add(new GroupedAlbumDto
            {
                BandName = primary.Band?.Name ?? string.Empty,
                AlbumName = primary.CanonicalTitle ?? primary.Name,
                PhotoUrl = photoUrl,
                ReleaseDate = primary.ReleaseDate,
                Genre = primary.Genre,
                Media = primary.Media,
                Status = primary.Status,
                CanonicalTitle = primary.CanonicalTitle,
                OriginalYear = primary.OriginalYear,
                Variants = variants
            });
        }

        return new PagedResultDto<GroupedAlbumDto>
        {
            Items = groupedDtos,
            TotalCount = totalCount,
            PageCount = pageCount,
            PageSize = pageSize,
            CurrentPage = page
        };
    }

    public async Task<AlbumDto?> GetAlbumById(Guid id, CancellationToken cancellationToken = default)
    {
        var album = await _albumRepository.GetAsync(id, cancellationToken);
        return album == null ? null : _mapper.Map<AlbumDto>(album);
    }

    private static bool AreAlbumsMatching(Data.Entities.AlbumEntity albumA, Data.Entities.AlbumEntity albumB)
    {
        if (string.IsNullOrWhiteSpace(albumA.CanonicalTitle) || string.IsNullOrWhiteSpace(albumB.CanonicalTitle))
        {
            return false;
        }

        if (albumA.Media != albumB.Media)
        {
            return false;
        }

        var bandA = albumA.Band?.Name?.Trim() ?? string.Empty;
        var bandB = albumB.Band?.Name?.Trim() ?? string.Empty;

        if (!bandA.Equals(bandB, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return albumA.CanonicalTitle.Trim().Equals(albumB.CanonicalTitle.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}