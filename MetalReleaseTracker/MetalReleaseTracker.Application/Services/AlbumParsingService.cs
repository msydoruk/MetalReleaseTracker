using MetalReleaseTracker.Application.DTOs;
using MetalReleaseTracker.Application.Interfaces;
using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Application.Services
{
    public class AlbumParsingService
    {
        private readonly IParserFactory _parserFactory;

        public AlbumParsingService(IParserFactory parserFactory)
        {
            _parserFactory = parserFactory;
        }

        public async Task<IEnumerable<AlbumDto>> GetAlbumsFromDistributor(DistributorCode distributorCode, string parsingUrl)
        {
            var parser = _parserFactory.CreateParser(distributorCode);

            var albums = await parser.ParseAlbums(parsingUrl);

            return albums.Data;
        }
    }
}
