# Testing Rules

## Structure

- Tests are embedded in each service under `Tests/` folder (not separate test projects)
- Unit tests: `Tests/UnitTests/`
- Integration tests: `Tests/IntegrationTests/`
- xUnit + Moq + Testcontainers (Docker required for integration tests)

## Test Naming

- Method naming: `MethodName_Scenario_ExpectedResult`
  - Example: `Consume_WhenConsumingNewAlbum_ShouldAddAlbumToDatabase`
  - Example: `ParseListings_FirstPage_ReturnsNonEmptyListings`
- Test classes: suffix with `Tests` (e.g., `AlbumProcessedEventConsumerTests`)
- Smoke tests: suffix with `SmokeTests` (e.g., `ParagonRecordsParserSmokeTests`)

## Patterns

- Use Arrange-Act-Assert pattern
- Create factories for test data (e.g., `AlbumFactory.CreateXxx()`) — don't inline large object creation
- Use `IClassFixture<TestPostgresDatabaseFixture>` for integration tests with real DB
- Use Moq for mocking interfaces (contexts, external services)
- Use xUnit assertions (`Assert.Equal`, `Assert.NotNull`, etc.) — NOT Fluent Assertions
- One test per scenario — not multiple unrelated assertions in one test
- Tests should be deterministic — no random data or time-dependent assertions

## Parser Smoke Tests

- Extend `ParserSmokeTestBase` from `Tests/IntegrationTests/Parsers/`
- Use `CreateHtmlDocumentLoader()` and `CreateSettingsService()` helper methods
- Use `NullLogger<T>.Instance` for logger
- Use `[Trait("Category", "SmokeLocal")]` attribute
- Three standard tests per parser:
  1. `ParseListings_FirstPage_ReturnsNonEmptyListings` — verify listings parse correctly
  2. `ParseListings_Pagination_ReturnsNextPageWithinSameCategory` — verify pagination works
  3. `ParseAlbumDetail_SingleProduct_ReturnsPopulatedFields` — verify detail parsing
- Use base class assertion helpers: `AssertListingPageValid()`, `AssertAlbumDetailValid()`, `AssertNextPageIsWithinSameCategory()`, `AssertPagesHaveDistinctListings()`
