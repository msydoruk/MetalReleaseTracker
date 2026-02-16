namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface IBandReferenceService
{
    Task SyncUkrainianBandsAsync(CancellationToken cancellationToken);
}
