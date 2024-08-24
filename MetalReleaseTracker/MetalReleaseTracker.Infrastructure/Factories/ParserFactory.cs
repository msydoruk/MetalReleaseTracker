using MetalReleaseTracker.Core.Entities;
using MetalReleaseTracker.Core.Interfaces;
using MetalReleaseTracker.Core.Parsers;
using MetalReleaseTracker.Infrastructure.Loaders;
using MetalReleaseTracker.Infrastructure.Parsers;

namespace MetalReleaseTracker.Infrastructure.Factories
{
    public class ParserFactory : IParserFactory
    {
        private readonly HtmlLoader _htmlLoader;
        private readonly MediaTypeParser _mediaTypeParser;
        private readonly AlbumStatusParser _albumStatusParser;
        private readonly YearParser _yearParser;

        public ParserFactory(HtmlLoader htmlLoader, MediaTypeParser mediaTypeParser, AlbumStatusParser albumStatusParser, YearParser yearParser)
        {
            _htmlLoader = htmlLoader;
            _mediaTypeParser = mediaTypeParser;
            _albumStatusParser = albumStatusParser;
            _yearParser = yearParser;
        }

        public IParser CreateParser(Distributor distributor)
        {
            return distributor.Name switch
            {
                "Osmose Productions" => new OsmoseProductionsParser(_htmlLoader, _mediaTypeParser, _albumStatusParser, _yearParser),
                _ => throw new NotSupportedException($"Parser for distributor {distributor.Name} is not supported."),
            };
        }
    }
}
