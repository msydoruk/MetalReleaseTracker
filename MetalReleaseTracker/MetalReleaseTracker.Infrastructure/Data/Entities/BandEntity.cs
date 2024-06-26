using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalReleaseTracker.Infrastructure.Data.Entities
{
    [Table("Bands")]
    public class BandEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "The band name is required.")]
        public string Name { get; set; }
    }
}
