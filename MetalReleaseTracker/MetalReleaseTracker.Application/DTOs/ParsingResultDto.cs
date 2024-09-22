using System;

namespace MetalReleaseTracker.Application.DTOs
{
    public class ParsingResultDto<T>
    {
        public T Data { get; set; }

        public bool IsSuccess { get; set; } = true;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
