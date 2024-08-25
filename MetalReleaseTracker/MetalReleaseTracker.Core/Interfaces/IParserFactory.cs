using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IParserFactory
    {
        IParser CreateParser(DistributorCode code);
    }
}
