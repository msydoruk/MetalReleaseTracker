namespace MetalReleaseTracker.CoreDataService.Data.Entities;

public class RefreshTokenEntity
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public string Token { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsUsed { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime Created { get; set; }
}