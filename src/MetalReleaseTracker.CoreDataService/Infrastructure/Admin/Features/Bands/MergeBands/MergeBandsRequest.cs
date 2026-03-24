namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.Bands.MergeBands;

public class MergeBandsRequest
{
    public Guid TargetBandId { get; set; }

    public Guid SourceBandId { get; set; }
}
