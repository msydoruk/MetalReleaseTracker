using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public record AiVerificationFilterDto
{
    public int Page { get; init; } = 1;

    public int PageSize { get; init; } = 25;

    public DistributorCode? DistributorCode { get; init; }

    public bool? IsUkrainian { get; init; }

    public bool? VerifiedOnly { get; init; }

    public AiVerificationSortField? SortBy { get; init; }

    public bool? SortAscending { get; init; }
}
