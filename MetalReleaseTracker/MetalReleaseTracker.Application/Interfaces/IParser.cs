using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Application.Interfaces
{
    public interface IParser
    {
        DistributorCode DistributorCode { get; }

        Task<IEnumerable<AlbumDTO>> ParseAlbums(string parsingUrl);
    }
}
