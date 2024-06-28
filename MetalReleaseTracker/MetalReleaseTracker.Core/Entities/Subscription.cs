﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetalReleaseTracker.Core.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public bool NotifyForNewReleases { get; set; }
    }
}