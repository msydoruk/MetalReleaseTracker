namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Authentication;

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; }

    public string UserId { get; set; }
}