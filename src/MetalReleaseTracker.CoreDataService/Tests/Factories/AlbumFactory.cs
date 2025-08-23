using MetalReleaseTracker.CoreDataService.Configuration;
using MetalReleaseTracker.CoreDataService.Data.Entities;
using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;
using MetalReleaseTracker.CoreDataService.Data.Events;

namespace MetalReleaseTracker.CoreDataService.Tests.Factories;

public static class AlbumFactory
{
    public static AlbumEntity CreateAlbumEntity(Guid id, string sku, Guid distributorId, Guid bandId)
    {
        return new AlbumEntity
        {
            Id = id,
            DistributorId = distributorId,
            BandId = bandId,
            SKU = sku,
            Name = "Fake Album Name",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2023, 12, 31), DateTimeKind.Utc),
            Genre = "Metal",
            Price = 19.99f,
            PurchaseUrl = "https://fakepurchaseurl.com",
            PhotoUrl = "https://fakephotourl.com",
            Media = AlbumMediaType.CD,
            Label = "Fake Label",
            Press = "Fake Press",
            Description = "This is a fake description of the album.",
            CreatedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            LastUpdateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            Status = AlbumStatus.New
        };
    }

    public static AlbumProcessedPublicationEvent CreateAlbumProcessedPublicationEvent(
        Guid id,
        string sku,
        DistributorCode distributorCode,
        string bandName,
        float price,
        AlbumProcessedStatus albumProcessedStatus)
    {
        return new AlbumProcessedPublicationEvent
        {
            Id = id,
            DistributorCode = distributorCode,
            BandName = bandName,
            SKU = sku,
            Name = "Fake Album Name",
            ReleaseDate = DateTime.SpecifyKind(new DateTime(2023, 12, 31), DateTimeKind.Utc),
            Genre = "Metal",
            Price = price,
            PurchaseUrl = "https://fakepurchaseurl.com",
            PhotoUrl = "https://fakephotourl.com",
            Media = AlbumMediaType.CD,
            Label = "Fake Label",
            Press = "Fake Press",
            Description = "This is a fake description of the album.",
            CreatedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            LastUpdateDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc),
            Status = AlbumStatus.New,
            ProcessedStatus = albumProcessedStatus
        };
    }
}