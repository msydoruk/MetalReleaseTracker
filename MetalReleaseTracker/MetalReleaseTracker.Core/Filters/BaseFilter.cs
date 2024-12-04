using System;

namespace MetalReleaseTracker.Core.Filters
{
    public class BaseFilter
    {
        public int Take { get; set; }

        public int Skip { get; set; }

        public string? OrderBy { get; set; }

        public bool Descending { get; set; }
    }
}
