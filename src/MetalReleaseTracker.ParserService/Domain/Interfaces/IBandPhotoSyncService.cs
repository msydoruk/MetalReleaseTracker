namespace MetalReleaseTracker.ParserService.Domain.Interfaces;

public interface IBandPhotoSyncService
{
    Task SyncBandPhotosAsync(CancellationToken cancellationToken);
}
