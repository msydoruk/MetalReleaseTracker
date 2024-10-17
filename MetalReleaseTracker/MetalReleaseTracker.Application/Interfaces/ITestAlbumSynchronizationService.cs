using System;

namespace MetalReleaseTracker.Application.Interfaces
{
    public interface ITestAlbumSynchronizationService
    {
        Task SynchronizeAllAlbums();
    }
}
