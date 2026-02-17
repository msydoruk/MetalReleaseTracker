namespace MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

public record ListingItem(string BandName, string AlbumTitle, string DetailUrl, string RawTitle, AlbumMediaType? MediaType = null);
