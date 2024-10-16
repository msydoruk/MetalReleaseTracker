using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Entities
{
    public class Distributor
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string ParsingUrl { get; set; }

        public DistributorCode Code { get; set; }
    }
}
