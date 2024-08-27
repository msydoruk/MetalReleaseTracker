using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IParser
    {
        DistributorCode DistributorCode { get; }

        Task<IEnumerable<Album>> ParseAlbums(string parsingUrl);
    }
}
