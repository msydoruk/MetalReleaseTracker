namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Authentication;

public class JwtTokenDto
{
    public string Token { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }
}