namespace MetalReleaseTracker.CoreDataService.Configuration;

public class GoogleAuthSettings
{
    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string CallbackPath { get; set; } = "/signin-google";

    public List<string> Scopes { get; set; } = new() { "email", "profile" };
}