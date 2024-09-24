using System;

namespace MetalReleaseTracker.Infrastructure.Parsers
{
    public class ParsingResult<T>
    {
        public T Data { get; set; }

        public bool IsSuccess { get; set; } = true;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
