using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.GetAlbums;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Albums.UpdateAlbum;

public class UpdateAlbumRequest
{
    public string? Name { get; set; }

    public string? Genre { get; set; }

    public float? Price { get; set; }

    public string? Status { get; set; }

    public string? StockStatus { get; set; }

    public string? Description { get; set; }

    public string? Label { get; set; }

    public string? Press { get; set; }

    public Dictionary<string, AlbumTranslationDto>? Translations { get; set; }
}
