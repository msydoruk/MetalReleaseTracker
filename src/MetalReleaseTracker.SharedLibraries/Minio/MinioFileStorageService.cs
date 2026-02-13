using MetalReleaseTracker.SharedLibraries.Helpers;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace MetalReleaseTracker.SharedLibraries.Minio;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinioFileStorageConfig _config;

    public MinioFileStorageService(IMinioClient minioClient, IOptions<MinioFileStorageConfig> options)
    {
        _minioClient = minioClient;
        _config = options.Value;
    }

    public async Task UploadFileAsync(string filePach, Stream fileStream, CancellationToken cancellationToken = default)
    {
        await EnsureBucketExistsAsync(cancellationToken);

        await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(filePach)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(ContentTypeHelper.GetContentType(filePach)),
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

    public async Task<string> GetFileUrlAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;

        try
        {
            var presignedUrl = await _minioClient.PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(filePath)
                .WithExpiry(60 * 60 * 24 * _config.PresignedUrlExpiryDays));
            
            return System.Web.HttpUtility.UrlDecode(presignedUrl);
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            await EnsureBucketExistsAsync(cancellationToken);

            await _minioClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(filePath), cancellationToken);

            return true;
        }
        catch (ObjectNotFoundException)
        {
            return false;
        }
    }
    
    private async Task<Stream> DownloadFileAsStreamAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var memoryStream = new MemoryStream();

        await _minioClient.GetObjectAsync(new GetObjectArgs()
            .WithBucket(_config.BucketName)
            .WithObject(filePath)
            .WithCallbackStream(stream => stream.CopyToAsync(memoryStream, cancellationToken)), cancellationToken);

        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        var bucketExists = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_config.BucketName), cancellationToken);

        if (!bucketExists)
        {
            await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_config.BucketName), cancellationToken);
        }
    }
}