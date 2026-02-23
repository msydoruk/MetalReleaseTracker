namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;

public interface ISettingsSeedService
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
