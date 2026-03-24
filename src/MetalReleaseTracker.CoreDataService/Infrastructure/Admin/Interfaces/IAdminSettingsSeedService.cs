namespace MetalReleaseTracker.CoreDataService.Infrastructure.Admin.Interfaces;

public interface IAdminSettingsSeedService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
