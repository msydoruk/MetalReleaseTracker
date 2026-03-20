namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class AlbumRatingDto
{
    public double? AverageRating { get; set; }

    public int RatingCount { get; set; }

    public int? UserRating { get; set; }
}
