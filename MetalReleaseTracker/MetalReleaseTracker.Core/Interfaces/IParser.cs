using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IParser
    {
        Task<IEnumerable<Album>> ParseAlbums(string parsingUrl);
    }
}
