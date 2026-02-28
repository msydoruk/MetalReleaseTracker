using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record ParsingSessionFilterDto
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 25;

    public DistributorCode? DistributorCode { get; init; }

    public AlbumParsingStatus? ParsingStatus { get; init; }

    public ParsingSessionSortField? SortBy { get; init; }

    public bool? SortAscending { get; init; }
}
