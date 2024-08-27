using MetalReleaseTracker.Core.Enums;
using MetalReleaseTracker.Core.Interfaces;

namespace MetalReleaseTracker.Infrastructure.Factories
{
    public class ParserFactory : IParserFactory
    {
        private readonly IEnumerable<IParser> _parsers;

        public ParserFactory(IEnumerable<IParser> parsers)
        {
            _parsers = parsers;
        }

        public IParser CreateParser(DistributorCode code)
        {
           var parser = _parsers.FirstOrDefault(p => p.DistributorCode == code);

            if (parser == null)
            {
                throw new NotSupportedException($"Parser for distributor {code} is not supported.");
            }

            return parser;
        }
    }
}