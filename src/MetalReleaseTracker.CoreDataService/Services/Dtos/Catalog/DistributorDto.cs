using MetalReleaseTracker.CoreDataService.Configuration;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class DistributorDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DistributorCode Code { get; set; }
}