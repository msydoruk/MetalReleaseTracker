using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record CatalogueDetailFilterDto
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 25;

    public DistributorCode? DistributorCode { get; init; }

    public ChangeType? ChangeType { get; init; }

    public PublicationStatus? PublicationStatus { get; init; }

    public string? Search { get; init; }

    public CatalogueDetailSortField? SortBy { get; init; }

    public bool? SortAscending { get; init; }
}
