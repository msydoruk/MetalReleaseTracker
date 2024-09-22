using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Application.Interfaces
{
    public interface IParser
    {
        DistributorCode DistributorCode { get; }

        Task<ParsingResultDto<List<AlbumDto>>> ParseAlbums(string parsingUrl);
    }
}
