namespace MetalReleaseTracker.SharedLibraries.Helpers;

public static class ContentTypeHelper
{
    private static readonly Dictionary<string, string> _contentTypes = new()
    {
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".webp", "image/webp" },
        { ".bmp", "image/bmp" },
        { ".tiff", "image/tiff" },
        { ".svg", "image/svg+xml" },
        { ".json", "application/json" },
        { ".txt", "text/plain" },
        { ".pdf", "application/pdf" },
        { ".xml", "application/xml" },
        { ".csv", "text/csv" }
    };

    public static string GetContentType(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return "application/octet-stream";

        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        
        return _contentTypes.TryGetValue(extension, out var contentType) 
            ? contentType 
            : "application/octet-stream";
    }
}