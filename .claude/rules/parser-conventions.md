# Parser Conventions

## File Structure per Distributor

Each distributor parser requires these files:
1. `Infrastructure/Parsers/{Name}Parser.cs` — parser implementation
2. `Infrastructure/Parsers/Selectors/{Name}Selectors.cs` — XPath/CSS selector constants
3. `Infrastructure/Parsers/Exceptions/{Name}ParserException.cs` — custom exception class
4. Registration in `Infrastructure/Parsers/Extensions/ParserRegistrationExtension.cs`
5. Enum value in `Domain/Models/ValueObjects/DistributorCode.cs`
6. Smoke tests in `Tests/IntegrationTests/Parsers/{Name}ParserSmokeTests.cs`

## Parser Types

### Standard (BaseDistributorParser)
- Extend `BaseDistributorParser` — handles multi-category crawling, pagination, delays
- Constructor injects: `IHtmlDocumentLoader`, `ISettingsService`, `ILogger<T>`
- Must override: `DistributorCode`, `CatalogueUrls`, `ParserName`, `ParseListingsFromPage()`, `ParseAlbumDetails()`, `FindNextPageLink()`, `CreateParserException()`, `IsOwnException()`
- Optionally override: `CategoryMediaTypes`, `TransformNextPageUrl()`
- Use `CurrentCategoryMediaType` to tag listings with the correct media type
- Use `LoadHtmlDocument()` (protected) to load pages with error handling

### FlareSolverr (Cloudflare-protected sites)
- Same as standard but registered with `ResolvedParameter` to inject `FlareSolverrHtmlDocumentLoader` instead of `HtmlDocumentLoader`

### Selenium (JavaScript-heavy sites)
- Implement `IListingParser` and `IAlbumDetailParser` directly (NOT BaseDistributorParser)
- Inject `ISeleniumWebDriverFactory` for browser management
- Create/dispose WebDriver per operation

## AlbumParsedEvent Fields

Required fields that must be populated:
- `DistributorCode` — the parser's distributor code
- `BandName` — band/artist name (split from title if combined)
- `SKU` — unique identifier (use `AlbumParsingHelper.GenerateSkuFromUrl()` if site has no SKU)
- `Name` — album title
- `Price` — parsed via `AlbumParsingHelper.ParsePrice()` (returns 0.0f if unparseable)
- `PurchaseUrl` — the detail page URL
- `PhotoUrl` — cover image URL (full absolute URL)

Optional but expected:
- `Genre`, `Label`, `Press`, `Description`, `Status` (PreOrder/New/Restock), `Media` (CD/LP/Tape)
- `CanonicalTitle`, `OriginalYear`

## Selectors Class

- Static class with `public const string` fields for each XPath selector
- Group by page type: listing page selectors, then detail page selectors
- Name selectors descriptively: `ProductLink`, `DetailPrice`, `NextPageLink`
- Include fallback selectors where applicable (e.g., `DetailTitleFallback`)

## Helpers

- `AlbumParsingHelper.ParsePrice(string)` — parses price text to float (InvariantCulture)
- `AlbumParsingHelper.GenerateSkuFromUrl(string)` — generates SKU from URL path
- `AlbumParsingHelper.ParseAlbumStatus(string)` — maps status text to `AlbumStatus` enum
- `AlbumParsingHelper.ParseMediaType(string)` — detects media type from text
- `AlbumParsingHelper.Truncate*(string?)` — truncates fields to DB column limits
- `ParserHelper.ExtractProductJsonLd(HtmlDocument)` — extracts JSON-LD Product schema
- `ParserHelper.StripHtml(string)` — strips HTML tags from text
- `ParserHelper.DelayBetweenRequestsAsync()` — applies configurable delay between requests

## Common Patterns

- Use `HtmlEntity.DeEntitize()` on all extracted text
- Use `processedUrls` HashSet to deduplicate listings
- Use `string.IsNullOrEmpty()` checks before processing nodes
- Convert relative URLs to absolute using base URL constant
- Parse title into (BandName, AlbumTitle) using ` - ` or dash separators
- Handle null XPath results gracefully — return empty string, not throw
