using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalReleaseTracker.Infrastructure.Data.Entities
{
    [Table("Distributors")]
    public class DistributorEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "The distributor  name is required.")]
        public string Name { get; set; }

        [Url]
        public string ParsingUrl { get; set; }
    }
}
