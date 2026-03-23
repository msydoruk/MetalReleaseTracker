using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class AlbumFormatGroupDto
{
    public AlbumMediaType? Media { get; set; }

    public List<AlbumVariantDto> Variants { get; set; } = [];
}
