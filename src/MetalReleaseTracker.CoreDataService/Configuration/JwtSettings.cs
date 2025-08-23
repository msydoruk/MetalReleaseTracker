namespace MetalReleaseTracker.CoreDataService.Configuration;

public class JwtSettings
{
    public string Issuer { get; set; }

    public string Audience { get; set; }

    public string Key { get; set; }

    public int ExpiresMinutes { get; set; }

    public int RefreshTokenExpirationDays { get; set; }
}