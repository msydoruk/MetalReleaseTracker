using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class AiVerificationDto
{
    public Guid Id { get; set; }

    public string BandName { get; set; } = string.Empty;

    public string AlbumTitle { get; set; } = string.Empty;

    public Guid? MatchedBandDiscographyId { get; set; }

    public string? MatchedAlbumTitle { get; set; }

    public string? MatchedAlbumType { get; set; }

    public int? MatchedAlbumYear { get; set; }

    public DistributorCode? DistributorCode { get; set; }

    public Guid? VerificationId { get; set; }

    public bool? IsUkrainian { get; set; }

    public double? ConfidenceScore { get; set; }

    public string? AiAnalysis { get; set; }

    public AiVerificationDecision? AdminDecision { get; set; }

    public DateTime? AdminDecisionDate { get; set; }

    public DateTime? VerifiedAt { get; set; }
}
