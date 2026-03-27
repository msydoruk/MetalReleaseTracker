namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Features.DataQuality.GetDataQualitySummary;

public class DataQualitySummaryDto
{
    public int AlbumsMissingCovers { get; set; }

    public int BandsMissingGenre { get; set; }

    public int BandsMissingPhoto { get; set; }

    public int PotentialDuplicateBands { get; set; }
}
