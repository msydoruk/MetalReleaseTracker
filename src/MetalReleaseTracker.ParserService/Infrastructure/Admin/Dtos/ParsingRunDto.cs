using System.Text.Json;
using System.Text.Json.Serialization;
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;

public class ParsingRunDto
{
    public Guid Id { get; set; }

    public ParsingJobType JobType { get; set; }

    public DistributorCode? DistributorCode { get; set; }

    public ParsingRunStatus Status { get; set; }

    public int TotalItems { get; set; }

    public int ProcessedItems { get; set; }

    public int FailedItems { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? ErrorMessage { get; set; }

    [JsonIgnore]
    public string? CountersJson { get; set; }

    public Dictionary<string, int>? Counters =>
        !string.IsNullOrEmpty(CountersJson)
            ? JsonSerializer.Deserialize<Dictionary<string, int>>(CountersJson)
            : null;
}
