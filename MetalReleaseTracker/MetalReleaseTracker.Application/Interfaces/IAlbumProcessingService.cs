using System;

namespace MetalReleaseTracker.Application.Interfaces
{
    public interface IAlbumProcessingService
    {
        Task SynchronizeAlbums();
    }
}
