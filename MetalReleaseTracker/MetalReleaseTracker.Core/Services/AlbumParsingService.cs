using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Core.Services
{
    public class AlbumParsingService
    {
        private readonly IParserFactory _parserFactory;

        public AlbumParsingService(IParserFactory parserFactory)
        {
            _parserFactory = parserFactory;
        }

        public async Task<IEnumerable<Album>> GetAlbumsFromDistributor(DistributorCode distributorCode, string parsingUrl)
        {
            var parser = _parserFactory.CreateParser(distributorCode);

            var albums = await parser.ParseAlbums(parsingUrl);

            return albums;
        }
    }
}
