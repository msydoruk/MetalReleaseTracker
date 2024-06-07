using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalReleaseTracker.Core.Entities
{
    public class BandEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "The band name is required.")]
        public string Name { get; set; }
    }
}
