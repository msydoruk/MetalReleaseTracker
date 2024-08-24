using MetalReleaseTracker.Core.Entities;

namespace MetalReleaseTracker.Core.Interfaces
{
    public interface IParserFactory
    {
        IParser CreateParser(Distributor distributor);
    }
}
