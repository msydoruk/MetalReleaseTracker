namespace MetalReleaseTracker.CoreDataService.Services.Configuration;

public class EmailServiceSettings
{
    public string ApiKey { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "Metal Release Tracker";
}
