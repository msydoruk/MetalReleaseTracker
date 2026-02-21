using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record CatalogueIndexFilterDto
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 25;

    public DistributorCode? DistributorCode { get; init; }

    public CatalogueIndexStatus? Status { get; init; }

    public string? Search { get; init; }

    public CatalogueIndexSortField? SortBy { get; init; }

    public bool? SortAscending { get; init; }
}
