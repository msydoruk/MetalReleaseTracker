using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Parsers;
using MetalReleaseTracker.Infrastructure.Loaders;
using MetalReleaseTracker.Infrastructure.Parsers;

namespace MetalReleaseTracker.Infrastructure.Factories
{
    public class ParserFactory : IParserFactory
    {
        private readonly HtmlLoader _htmlLoader;
        private readonly AlbumParser _albumParser;

        public ParserFactory(HtmlLoader htmlLoader, AlbumParser albumParser)
        {
            _htmlLoader = htmlLoader;
            _albumParser = albumParser;
        }

        public IParser CreateParser(DistributorCode code)
        {
            return code switch
            {
                DistributorCode.OsmoseProductions => new OsmoseProductionsParser(_htmlLoader, _albumParser),
                _ => throw new NotSupportedException($"Parser for distributor {code} is not supported."),
            };
        }
    }
}
