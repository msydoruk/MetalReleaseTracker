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

        if (filter.SortBy == Data.Entities.Enums.AlbumSortField.StoreCount)
        {
            groups = filter.SortAscending
                ? groups.OrderBy(group => group.Count).ToList()
                : groups.OrderByDescending(group => group.Count).ToList();
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
                PurchaseUrl = album.PurchaseUrl,
                StockStatus = album.StockStatus
            }).ToList();

            groupedDtos.Add(new GroupedAlbumDto
            {
                BandId = primary.BandId,
                BandName = primary.Band?.Name ?? string.Empty,
                BandSlug = primary.Band?.Slug ?? string.Empty,
                AlbumName = primary.CanonicalTitle ?? primary.Name,
                AlbumSlug = primary.Slug,
                PhotoUrl = photoUrl,
                Genre = primary.Genre,
                Media = primary.Media,
                Status = primary.Status,
                StockStatus = primary.StockStatus,
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
        if (album == null)
        {
            return null;
        }

        var dto = _mapper.Map<AlbumDto>(album);
        dto.PhotoUrl = await _fileStorageService.GetFileUrlAsync(album.PhotoUrl, cancellationToken);
        return dto;
    }

    public async Task<AlbumDetailDto?> GetAlbumDetail(Guid id, CancellationToken cancellationToken = default)
    {
        var album = await _albumRepository.GetAsync(id, cancellationToken);
        if (album == null)
        {
            return null;
        }

        var photoUrl = await _fileStorageService.GetFileUrlAsync(album.PhotoUrl, cancellationToken);

        List<AlbumVariantDto> variants;
        if (!string.IsNullOrWhiteSpace(album.CanonicalTitle))
        {
            var matchingAlbums = await _albumRepository.GetMatchingAlbumsAsync(
                album.CanonicalTitle, album.Media, album.BandId, cancellationToken);

            variants = matchingAlbums.Select(matchingAlbum => new AlbumVariantDto
            {
                AlbumId = matchingAlbum.Id,
                DistributorId = matchingAlbum.DistributorId,
                DistributorName = matchingAlbum.Distributor?.Name ?? string.Empty,
                Price = matchingAlbum.Price,
                PurchaseUrl = matchingAlbum.PurchaseUrl,
                StockStatus = matchingAlbum.StockStatus
            }).ToList();
        }
        else
        {
            variants =
            [
                new AlbumVariantDto
                {
                    AlbumId = album.Id,
                    DistributorId = album.DistributorId,
                    DistributorName = album.Distributor?.Name ?? string.Empty,
                    Price = album.Price,
                    PurchaseUrl = album.PurchaseUrl,
                    StockStatus = album.StockStatus
                }
            ];
        }

        var bandAlbums = await _albumRepository.GetAlbumsByBandIdAsync(album.BandId, cancellationToken);

        var relatedGroups = new List<List<Data.Entities.AlbumEntity>>();
        var assignedIndices = new HashSet<int>();

        for (var i = 0; i < bandAlbums.Count; i++)
        {
            if (assignedIndices.Contains(i))
            {
                continue;
            }

            var group = new List<Data.Entities.AlbumEntity> { bandAlbums[i] };
            assignedIndices.Add(i);

            for (var j = i + 1; j < bandAlbums.Count; j++)
            {
                if (!assignedIndices.Contains(j) && AreAlbumsMatching(bandAlbums[i], bandAlbums[j]))
                {
                    group.Add(bandAlbums[j]);
                    assignedIndices.Add(j);
                }
            }

            relatedGroups.Add(group);
        }

        var currentCanonical = album.CanonicalTitle?.Trim().ToLower();
        var relatedReleases = new List<RelatedAlbumDto>();
        foreach (var group in relatedGroups)
        {
            var primary = group[0];
            var groupCanonical = primary.CanonicalTitle?.Trim().ToLower();

            if (currentCanonical != null && groupCanonical == currentCanonical && primary.Media == album.Media)
            {
                continue;
            }

            var relatedPhotoUrl = await _fileStorageService.GetFileUrlAsync(primary.PhotoUrl, cancellationToken);
            relatedReleases.Add(new RelatedAlbumDto
            {
                AlbumId = primary.Id,
                AlbumName = primary.CanonicalTitle ?? primary.Name,
                AlbumSlug = primary.Slug,
                PhotoUrl = relatedPhotoUrl,
                Media = primary.Media,
                OriginalYear = primary.OriginalYear,
                MinPrice = group.Min(albumEntity => albumEntity.Price)
            });

            if (relatedReleases.Count >= 10)
            {
                break;
            }
        }

        string? bandPhotoUrl = null;
        if (album.Band?.PhotoUrl != null)
        {
            bandPhotoUrl = await _fileStorageService.GetFileUrlAsync(album.Band.PhotoUrl, cancellationToken);
        }

        return new AlbumDetailDto
        {
            PrimaryAlbumId = album.Id,
            AlbumName = album.CanonicalTitle ?? album.Name,
            Slug = album.Slug,
            PhotoUrl = photoUrl,
            Genre = album.Genre,
            Media = album.Media,
            Status = album.Status,
            StockStatus = album.StockStatus,
            CanonicalTitle = album.CanonicalTitle,
            OriginalYear = album.OriginalYear,
            Description = album.Description,
            Label = album.Label,
            Press = album.Press,
            BandId = album.BandId,
            BandName = album.Band?.Name ?? string.Empty,
            BandSlug = album.Band?.Slug ?? string.Empty,
            BandPhotoUrl = bandPhotoUrl,
            BandGenre = album.Band?.Genre,
            BandMetalArchivesUrl = album.Band?.MetalArchivesUrl,
            Variants = variants,
            RelatedReleases = relatedReleases
        };
    }

    public async Task<List<AlbumSuggestionDto>> GetSuggestions(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            return [];
        }

        var albums = await _albumRepository.GetFilteredAlbumsAsync(
            new AlbumFilterDto { Name = query, PageSize = 5, Page = 1 }, cancellationToken);

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var suggestions = new List<AlbumSuggestionDto>();

        foreach (var album in albums.Items)
        {
            if (album.Band != null && seen.Add(album.Band.Name))
            {
                suggestions.Add(new AlbumSuggestionDto
                {
                    Text = album.Band.Name,
                    Type = "band",
                    Id = album.BandId,
                    Slug = album.Band.Slug
                });
            }

            var albumName = album.CanonicalTitle ?? album.Name;
            if (seen.Add(albumName))
            {
                suggestions.Add(new AlbumSuggestionDto
                {
                    Text = albumName,
                    Type = "album",
                    Id = album.Id,
                    Slug = album.Slug
                });
            }

            if (suggestions.Count >= 7)
            {
                break;
            }
        }

        return suggestions;
    }

    public async Task<AlbumDto?> GetAlbumBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var album = await _albumRepository.GetBySlugAsync(slug, cancellationToken);
        if (album == null)
        {
            return null;
        }

        var dto = _mapper.Map<AlbumDto>(album);
        dto.PhotoUrl = await _fileStorageService.GetFileUrlAsync(album.PhotoUrl, cancellationToken);
        return dto;
    }

    public async Task<AlbumDetailDto?> GetAlbumDetailBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var album = await _albumRepository.GetBySlugAsync(slug, cancellationToken);
        if (album == null)
        {
            return null;
        }

        return await GetAlbumDetail(album.Id, cancellationToken);
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