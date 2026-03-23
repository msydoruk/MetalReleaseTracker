using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Dtos;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Entities;
using MetalReleaseTracker.ParserService.Infrastructure.Admin.Interfaces;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Configuration;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers.Helpers;
using MetalReleaseTracker.ParserService.Infrastructure.Services;
using MetalReleaseTracker.ParserService.Infrastructure.Services.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.IntegrationTests.Parsers;

[Trait("Category", "SmokeLocal")]
public class BlackMetalStoreParserSmokeTests : ParserSmokeTestBase
{
    private const string StartUrl = "https://blackmetalstore.com/categoria-produto/cds/";
    private const string FlareSolverrUrl = "http://localhost:8191";

    private static BlackMetalStoreParser CreateParser()
    {
        var settingsService = new FlareSolverrStubSettingsService();

        var flareSolverrClient = new FlareSolverrClient(
            new HttpClient(),
            settingsService,
            NullLogger<FlareSolverrClient>.Instance);

        var documentLoader = new FlareSolverrHtmlDocumentLoader(
            flareSolverrClient,
            NullLogger<FlareSolverrHtmlDocumentLoader>.Instance);

        return new BlackMetalStoreParser(
            documentLoader,
            CreateSettingsService(),
            NullLogger<BlackMetalStoreParser>.Instance);
    }

    [Fact]
    public async Task ParseListings_FirstPage_ReturnsNonEmptyListings()
    {
        await EnsureFlareSolverrAvailableAsync();
        var parser = CreateParser();

        var result = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);

        AssertListingPageValid(result);
    }

    [Fact]
    public async Task ParseListings_Pagination_ReturnsNextPageWithinSameCategory()
    {
        await EnsureFlareSolverrAvailableAsync();
        var parser = CreateParser();

        var firstPage = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);
        AssertNextPageIsWithinSameCategory(StartUrl, firstPage.NextPageUrl);

        var secondPage = await parser.ParseListingsAsync(firstPage.NextPageUrl!, CancellationToken.None);
        AssertListingPageValid(secondPage);
        AssertPagesHaveDistinctListings(firstPage, secondPage);
    }

    [Fact]
    public async Task ParseAlbumDetail_SingleProduct_ReturnsPopulatedFields()
    {
        await EnsureFlareSolverrAvailableAsync();
        var parser = CreateParser();

        var firstPage = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);
        Assert.True(firstPage.Listings.Count > 0);

        var detailUrl = firstPage.Listings[0].DetailUrl;
        var album = await parser.ParseAlbumDetailAsync(detailUrl, CancellationToken.None);

        AssertAlbumDetailValid(album, DistributorCode.BlackMetalStore);
    }

    [Fact]
    public async Task ParseAlbumDetail_SingleProduct_StockStatusIsPopulated()
    {
        await EnsureFlareSolverrAvailableAsync();
        var parser = CreateParser();

        var firstPage = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);
        Assert.True(firstPage.Listings.Count > 0);

        var detailUrl = firstPage.Listings[0].DetailUrl;
        var album = await parser.ParseAlbumDetailAsync(detailUrl, CancellationToken.None);

        AssertAlbumDetailStockStatusPopulated(album);
    }

    private static async Task EnsureFlareSolverrAvailableAsync()
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        try
        {
            await httpClient.GetAsync(FlareSolverrUrl);
        }
        catch (Exception exception)
        {
            Assert.Fail($"FlareSolverr is not available at {FlareSolverrUrl}. " +
                $"Start shared infrastructure first: docker compose -f src/MetalReleaseTracker.SharedInfrastructure/docker-compose.yml up -d. " +
                $"Error: {exception.Message}");
        }
    }

    private class FlareSolverrStubSettingsService : ISettingsService
    {
        public Task<FlareSolverrSettings> GetFlareSolverrSettingsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new FlareSolverrSettings
            {
                BaseUrl = FlareSolverrUrl,
                MaxTimeoutMs = 60000,
            });
        }

        public Task<GeneralParserSettings> GetGeneralParserSettingsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new GeneralParserSettings
            {
                MinDelayBetweenRequestsSeconds = 0,
                MaxDelayBetweenRequestsSeconds = 1,
            });
        }

        public Task<List<ParsingSourceEntity>> GetEnabledParsingSourcesAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new List<ParsingSourceEntity>());

        public Task<ParsingSourceEntity?> GetParsingSourceByCodeAsync(DistributorCode distributorCode, CancellationToken cancellationToken) =>
            Task.FromResult<ParsingSourceEntity?>(null);

        public Task<BandReferenceSettings> GetBandReferenceSettingsAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new BandReferenceSettings());

        public Task<List<AiAgentDto>> GetAiAgentsAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new List<AiAgentDto>());

        public Task<AiAgentDto?> GetAiAgentByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<AiAgentDto?>(null);

        public Task<AiAgentEntity?> GetActiveAiAgentAsync(CancellationToken cancellationToken) =>
            Task.FromResult<AiAgentEntity?>(null);

        public Task<AiAgentDto> CreateAiAgentAsync(CreateAiAgentDto dto, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<AiAgentDto?> UpdateAiAgentAsync(Guid id, UpdateAiAgentDto dto, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<bool> DeleteAiAgentAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<List<ParsingSourceDto>> GetParsingSourcesAsync(CancellationToken cancellationToken) =>
            Task.FromResult(new List<ParsingSourceDto>());

        public Task<ParsingSourceDto?> UpdateParsingSourceAsync(Guid id, UpdateParsingSourceDto dto, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CategorySettingsDto> GetSettingsByCategoryAsync(string category, CancellationToken cancellationToken) =>
            Task.FromResult(new CategorySettingsDto(new Dictionary<string, string>()));

        public Task<CategorySettingsDto> UpdateSettingsByCategoryAsync(string category, CategorySettingsDto dto, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }
}
