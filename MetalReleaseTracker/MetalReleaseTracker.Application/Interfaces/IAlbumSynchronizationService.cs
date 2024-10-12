using System;

namespace MetalReleaseTracker.Application.Interfaces
{
    public interface IAlbumSynchronizationService
    {
        Task SynchronizeAllAlbums();
    }
}
