using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace MetalReleaseTracker.SharedLibraries.Minio;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    public MinioFileStorageService(IMinioClient minioClient, IOptions<MinioFileStorageConfig> options)
    {
        _minioClient = minioClient;
        _bucketName = options.Value.BucketName;
    }

    public async Task UploadFileAsync(string filePach, Stream fileStream, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(filePach)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType("application/json"),
            cancellationToken);
    }

    public async Task<string> DownloadFileAsStringAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var memoryStream = await DownloadFileAsStreamAsync(filePath, cancellationToken);
        using var streamReader = new StreamReader(memoryStream);
        var content = await streamReader.ReadToEndAsync(cancellationToken);

        return content;
    }

    public async Task<List<string>> DownloadFileAsListAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await using var memoryStream = await DownloadFileAsStreamAsync(filePath, cancellationToken);
        using var streamReader = new StreamReader(memoryStream);
        
        var lines = new List<string>();
        while (await streamReader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line.Trim());
            }
        }
        
        return lines;
    }
    
    private async Task<Stream> DownloadFileAsStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_bucketName)
            .WithObject(filePath)
            .WithCallbackStream(stream => stream.CopyToAsync(memoryStream, cancellationToken)), cancellationToken);

        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName), cancellationToken);

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName), cancellationToken);
        }
    }
}