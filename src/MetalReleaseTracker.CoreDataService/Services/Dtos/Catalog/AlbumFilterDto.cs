using MetalReleaseTracker.CoreDataService.Data.Entities.Enums;

namespace MetalReleaseTracker.CoreDataService.Services.Dtos.Catalog;

public record AlbumFilterDto
{
    public string? Name { get; init; }

    public decimal? MinPrice { get; init; }

    public decimal? MaxPrice { get; init; }

    public DateTime? ReleaseDateFrom { get; init; }

    public DateTime? ReleaseDateTo { get; init; }

    public Guid? BandId { get; init; }

    public Guid? DistributorId { get; init; }

    public AlbumMediaType? MediaType { get; init; }

    public AlbumStatus? Status { get; init; }

    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 10;

    public AlbumSortField? SortBy { get; init; } = AlbumSortField.ReleaseDate;

    public bool SortAscending { get; init; } = true;
}