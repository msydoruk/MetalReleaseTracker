namespace MetalReleaseTracker.SharedLibraries.Minio;

public interface IFileStorageService
{
    Task UploadFileAsync(string filePath, Stream fileStream, CancellationToken cancellationToken = default);
    
    Task<string> DownloadFileAsStringAsync(string filePath, CancellationToken cancellationToken = default);
    
    Task<List<string>> DownloadFileAsListAsync(string filePath, CancellationToken cancellationToken = default);
    
    Task<string> GetFileUrlAsync(string filePath, CancellationToken cancellationToken = default);
    
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);

    Task<Stream> DownloadFileAsync(string filePath, CancellationToken cancellationToken = default);

    Task<List<string>> ListObjectsAsync(string prefix, CancellationToken cancellationToken = default);
}