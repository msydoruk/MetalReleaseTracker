using MetalReleaseTracker.Application.Interfaces;

namespace MetalReleaseTracker.Application.Services
{
    public class TestAlbumSynchronizationService : ITestAlbumSynchronizationService
    {
        public async Task SynchronizeAllAlbums()
        {
            await Task.Delay(500);
        }
    }
}
