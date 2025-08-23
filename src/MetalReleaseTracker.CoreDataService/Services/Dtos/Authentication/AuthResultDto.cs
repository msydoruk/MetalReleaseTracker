namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Authentication;

public class AuthResultDto
{
    public bool Success { get; set; }

    public string Message { get; set; }

    public Dictionary<string, string> Claims { get; set; }

    public string Token { get; set; }

    public string RefreshToken { get; set; }

    public DateTime Expiration { get; set; }
}