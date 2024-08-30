using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
