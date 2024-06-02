﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalReleaseTracker.Core.Entities
{
    public class DistributorEntity
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The distributor  name is required.")]
        public string Name { get; set; }

        [Url]
        public string ParsingUrl { get; set; }
    }
}