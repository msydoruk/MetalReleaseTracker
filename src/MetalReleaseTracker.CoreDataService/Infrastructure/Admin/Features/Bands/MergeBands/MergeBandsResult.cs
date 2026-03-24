using MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.GetBandById;

namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.MergeBands;

public class MergeBandsResult
{
    public bool NotFound { get; set; }

    public AdminBandDetailDto? Detail { get; set; }
}
