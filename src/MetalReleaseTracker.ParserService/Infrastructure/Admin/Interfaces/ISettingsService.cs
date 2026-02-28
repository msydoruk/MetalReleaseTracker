using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Services.Configuration;

namespace MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;

public interface ISettingsService
{
    Task<List<ParsingSourceEntity>> GetEnabledParsingSourcesAsync(CancellationToken cancellationToken);

    Task<ParsingSourceEntity?> GetParsingSourceByCodeAsync(DistributorCode distributorCode, CancellationToken cancellationToken);

    Task<GeneralParserSettings> GetGeneralParserSettingsAsync(CancellationToken cancellationToken);

    Task<BandReferenceSettings> GetBandReferenceSettingsAsync(CancellationToken cancellationToken);

    Task<FlareSolverrSettings> GetFlareSolverrSettingsAsync(CancellationToken cancellationToken);

    Task<List<AiAgentDto>> GetAiAgentsAsync(CancellationToken cancellationToken);

    Task<AiAgentDto?> GetAiAgentByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AiAgentEntity?> GetActiveAiAgentAsync(CancellationToken cancellationToken);

    Task<AiAgentDto> CreateAiAgentAsync(CreateAiAgentDto dto, CancellationToken cancellationToken);

    Task<AiAgentDto?> UpdateAiAgentAsync(Guid id, UpdateAiAgentDto dto, CancellationToken cancellationToken);

    Task<bool> DeleteAiAgentAsync(Guid id, CancellationToken cancellationToken);

    Task<List<ParsingSourceDto>> GetParsingSourcesAsync(CancellationToken cancellationToken);

    Task<ParsingSourceDto?> UpdateParsingSourceAsync(Guid id, UpdateParsingSourceDto dto, CancellationToken cancellationToken);

    Task<CategorySettingsDto> GetSettingsByCategoryAsync(string category, CancellationToken cancellationToken);

    Task<CategorySettingsDto> UpdateSettingsByCategoryAsync(string category, CategorySettingsDto dto, CancellationToken cancellationToken);
}
