using System.ComponentModel.DataAnnotations;

namespace MetalReleaseTracker.CoreDataService.Configuration;

public enum DistributorCode
{
    [Display(Name = "Osmose Productions")]
    OsmoseProductions = 1,

    [Display(Name = "Drakkar Records")]
    Drakkar = 2,

    [Display(Name = "Black Metal Vendor")]
    BlackMetalVendor = 3,

    [Display(Name = "Black Metal Store")]
    BlackMetalStore = 4,

    [Display(Name = "Napalm Records")]
    NapalmRecords = 5,

    [Display(Name = "Season of Mist")]
    SeasonOfMist = 6,

    [Display(Name = "Paragon Records")]
    ParagonRecords = 7
}