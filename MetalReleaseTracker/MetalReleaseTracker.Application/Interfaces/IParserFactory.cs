using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Application.Interfaces
{
    public interface IParserFactory
    {
        IParser CreateParser(DistributorCode code);
    }
}
