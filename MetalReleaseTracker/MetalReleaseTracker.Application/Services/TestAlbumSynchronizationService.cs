using MetalReleaseTracker.Application.Interfaces;

namespace MetalReleaseTracker.Application.Services
{
    public class TestAlbumSynchronizationService : IAlbumSynchronizationService
    {
        public async Task SynchronizeAllAlbums()
        {
            await Task.Delay(500);
        }
    }
}
