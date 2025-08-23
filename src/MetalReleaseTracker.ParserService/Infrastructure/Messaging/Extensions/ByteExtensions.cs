namespace MetalReleaseTracker.ParserService.Infrastructure.Messaging.Extensions;

public static class ByteExtensions
{
    public static List<byte[]> SplitIntoChunks(this byte[] bytes, int maxChunkSizeInBytes)
    {
        if (bytes == null || maxChunkSizeInBytes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxChunkSizeInBytes), "Chunk size must be greater than 0.");

        var chunks = new List<byte[]>();

        for (var i = 0; i < bytes.Length; i += maxChunkSizeInBytes)
        {
            var size = Math.Min(maxChunkSizeInBytes, bytes.Length - i);
            var chunk = new byte[size];
            Array.Copy(bytes, i, chunk, 0, size);
            chunks.Add(chunk);
        }

        return chunks;
    }
}