namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record BandReferenceFilterDto
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 25;

    public string? Search { get; init; }

    public BandReferenceSortField? SortBy { get; init; }

    public bool? SortAscending { get; init; }
}
