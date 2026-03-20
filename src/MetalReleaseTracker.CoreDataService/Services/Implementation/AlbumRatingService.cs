using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Repositories.Interfaces;
using MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;
using MetalReleaseTracker.CoreDataService.Services.Interfaces;

namespace MetalReleaseTracker.CoreDataService.Services.Implementation;

public class AlbumRatingService : IAlbumRatingService
{
    private readonly IAlbumRatingRepository _albumRatingRepository;

    public AlbumRatingService(IAlbumRatingRepository albumRatingRepository)
    {
        _albumRatingRepository = albumRatingRepository;
    }

    public async Task SubmitRatingAsync(string userId, Guid albumId, int rating, CancellationToken cancellationToken = default)
    {
        var entity = new AlbumRatingEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AlbumId = albumId,
            Rating = rating,
            CreatedDate = DateTime.UtcNow
        };

        await _albumRatingRepository.AddOrUpdateAsync(entity, cancellationToken);
    }

    public async Task<AlbumRatingDto> GetRatingAsync(Guid albumId, string? userId = null, CancellationToken cancellationToken = default)
    {
        var averageRating = await _albumRatingRepository.GetAverageRatingAsync(albumId, cancellationToken);
        var ratingCount = await _albumRatingRepository.GetRatingCountAsync(albumId, cancellationToken);

        int? userRating = null;
        if (!string.IsNullOrEmpty(userId))
        {
            var userEntity = await _albumRatingRepository.GetAsync(userId, albumId, cancellationToken);
            userRating = userEntity?.Rating;
        }

        return new AlbumRatingDto
        {
            AverageRating = averageRating,
            RatingCount = ratingCount,
            UserRating = userRating
        };
    }

    public async Task DeleteRatingAsync(string userId, Guid albumId, CancellationToken cancellationToken = default)
    {
        await _albumRatingRepository.DeleteAsync(userId, albumId, cancellationToken);
    }
}
