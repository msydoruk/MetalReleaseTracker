using MetalReleaseTracker.Core.Enums;

namespace MetalReleaseTracker.Core.Parsers
{
    public class MediaTypeParser
    {
        public MediaType ParseMediaType(string mediaType)
        {
            return mediaType switch
            {
                "CD" => MediaType.CD,
                "LP" => MediaType.LP,
                "Tape" => MediaType.Tape,
                _ => MediaType.Unknown
            };
        }
    }
}
