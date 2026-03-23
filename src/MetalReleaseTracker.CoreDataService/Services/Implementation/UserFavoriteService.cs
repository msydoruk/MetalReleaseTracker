using System.Globalization;
using System.Text;
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

    public async Task<byte[]> ExportCollectionAsync(string userId, string format, CancellationToken cancellationToken = default)
    {
        var favoriteAlbums = await _userFavoriteRepository.GetAllFavoriteAlbumsAsync(userId, cancellationToken);

        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Band,Album,Year,Genre,Format,Status,Price,Distributor,Purchase URL");

        foreach (var (favorite, album) in favoriteAlbums)
        {
            var bandName = EscapeCsvField(album.Band?.Name ?? string.Empty);
            var albumName = EscapeCsvField(album.Name);
            var year = album.OriginalYear?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            var genre = EscapeCsvField(album.Genre ?? string.Empty);
            var mediaFormat = FormatMediaType(album.Media);
            var collectionStatus = FormatCollectionStatus(favorite.Status);
            var price = album.Price.ToString("F2", CultureInfo.InvariantCulture);
            var distributor = EscapeCsvField(album.Distributor?.Name ?? string.Empty);
            var purchaseUrl = EscapeCsvField(album.PurchaseUrl);

            csvBuilder.AppendLine($"{bandName},{albumName},{year},{genre},{mediaFormat},{collectionStatus},{price},{distributor},{purchaseUrl}");
        }

        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csvBuilder.ToString())).ToArray();
    }

    private static string EscapeCsvField(string field)
    {
        if (field.Contains('"') || field.Contains(',') || field.Contains('\n') || field.Contains('\r'))
        {
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        return field;
    }

    private static string FormatMediaType(AlbumMediaType? mediaType)
    {
        return mediaType switch
        {
            AlbumMediaType.CD => "CD",
            AlbumMediaType.LP => "Vinyl",
            AlbumMediaType.Tape => "Tape",
            _ => string.Empty
        };
    }

    private static string FormatCollectionStatus(UserCollectionStatus status)
    {
        return status switch
        {
            UserCollectionStatus.Favorite => "Favorite",
            UserCollectionStatus.Want => "Wishlist",
            UserCollectionStatus.Owned => "Owned",
            _ => string.Empty
        };
    }
}
