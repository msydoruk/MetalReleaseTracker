namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Authentication;

public class LoginResultDto
{
    public bool Success { get; set; }

    public string Message { get; set; }

    public string RedirectUrl { get; set; }

    public string Token { get; set; }

    public string RefreshToken { get; set; }

    public Dictionary<string, string> Claims { get; set; }
}