namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Configuration;

public class AdminAuthSettings
{
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string JwtKey { get; set; } = string.Empty;

    public string JwtIssuer { get; set; } = "ParserService";

    public string JwtAudience { get; set; } = "ParserServiceAdmin";

    public int TokenExpirationMinutes { get; set; } = 480;
}
