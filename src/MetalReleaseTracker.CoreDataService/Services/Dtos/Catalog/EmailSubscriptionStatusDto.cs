namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class EmailSubscriptionStatusDto
{
    public bool IsSubscribed { get; set; }

    public bool IsVerified { get; set; }

    public string? Email { get; set; }
}
