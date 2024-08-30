using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Application.Interfaces
{
    public interface IParser
    {
        DistributorCode DistributorCode { get; }

        Task<IEnumerable<AlbumDto>> ParseAlbums(string parsingUrl);
    }
}
