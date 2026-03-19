namespace MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

public enum ParsingJobType
{
    DetailParsing = 0,
    CatalogueIndex = 1,
    BandReferenceSync = 2,
    AlbumPublisher = 3,
    BandPhotoSync = 4
}
