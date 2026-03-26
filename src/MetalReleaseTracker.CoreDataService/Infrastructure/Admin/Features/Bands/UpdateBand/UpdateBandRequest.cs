using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBands;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.UpdateBand;

public class UpdateBandRequest
{
    public string? Name { get; set; }

    public string? Genre { get; set; }

    public string? PhotoUrl { get; set; }

    public string? MetalArchivesUrl { get; set; }

    public int? FormationYear { get; set; }

    public bool? IsVisible { get; set; }

    public Dictionary<string, BandTranslationDto>? Translations { get; set; }
}
