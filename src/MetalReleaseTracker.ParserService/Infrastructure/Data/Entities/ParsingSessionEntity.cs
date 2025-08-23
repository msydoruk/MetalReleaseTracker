using System.ComponentModel.DataAnnotations;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Data.Entities.Enums;

namespace MetalReleaseTracker.ParserService.Infrastructure.Data.Entities;

public class ParsingSessionEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public DistributorCode DistributorCode { get; set; }

    [Required]
    public string PageToProcess { get; set; }

    [Required]
    public DateTime LastUpdatedDate { get; set; }

    public AlbumParsingStatus ParsingStatus { get; set; }
}