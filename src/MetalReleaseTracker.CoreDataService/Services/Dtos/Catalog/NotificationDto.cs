namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public class NotificationDto
{
    public Guid Id { get; set; }

    public Guid AlbumId { get; set; }

    public string AlbumName { get; set; } = string.Empty;

    public string BandName { get; set; } = string.Empty;

    public string? AlbumSlug { get; set; }

    public string? PhotoUrl { get; set; }

    public string NotificationType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedDate { get; set; }
}
