using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalReleaseTracker.Core.Entities
{
    public class Band
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
