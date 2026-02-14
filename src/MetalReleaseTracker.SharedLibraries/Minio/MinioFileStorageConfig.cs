namespace MetalReleaseTracker.SharedLibraries.Minio;

public class MinioFileStorageConfig
{
    public string Endpoint { get; set; }

    public string AccessKey { get; set; }

    public string SecretKey { get; set; }

    public string BucketName { get; set; }

    public string Region { get; set; }

    public string PublicEndpoint { get; set; }

    public int PresignedUrlExpiryDays { get; set; }
}