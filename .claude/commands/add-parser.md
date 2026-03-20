# Add New Distributor Parser

You are adding a new distributor parser to the ParserService. Follow these steps exactly.

## Input Required

Ask the user for:
1. **Distributor name** (e.g., "NuclearBlast")
2. **Website URL** — the catalogue/shop page to parse
3. **HTML loading strategy**: Standard HTTP (default), FlareSolverr (Cloudflare-protected), or Selenium (JavaScript-heavy)
4. **Category URLs** — separate URLs for CD, LP, Tape pages (or single URL if all-in-one)

## Step 1: Analyze the Target Website

Before writing any code, use Playwright browser tools to:
1. Navigate to the catalogue URL
2. Take a snapshot of the listing page
3. Identify the HTML structure: product containers, links, titles, pagination
4. Navigate to a product detail page
5. Take a snapshot of the detail page
6. Identify: title, price, image, label, genre, status, SKU elements
7. Document the XPath selectors needed

## Step 2: Add Enum Value

File: `src/MetalReleaseTracker.ParserService/Domain/Models/ValueObjects/DistributorCode.cs`

Add the next sequential enum value:
```csharp
{NewDistributorName} = {next_number}
```

## Step 3: Create Selectors Class

File: `src/MetalReleaseTracker.ParserService/Infrastructure/Parsers/Selectors/{Name}Selectors.cs`

```csharp
namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Selectors;

public static class {Name}Selectors
{
    // Listing page
    public const string ProductContainer = "...";
    public const string ProductLink = "...";
    public const string ProductTitle = "...";
    public const string NextPageLink = "...";

    // Detail page
    public const string DetailTitle = "...";
    public const string DetailPrice = "...";
    public const string DetailPhoto = "...";
    public const string DetailLabel = "...";
    public const string DetailDescription = "...";
    public const string DetailCartButton = "...";
}
```

Use XPath selectors identified from Step 1. Include fallback selectors where elements may vary.

## Step 4: Create Exception Class

File: `src/MetalReleaseTracker.ParserService/Infrastructure/Parsers/Exceptions/{Name}ParserException.cs`

```csharp
namespace MetalReleaseTracker.ParserService.Infrastructure.Parsers.Exceptions;

public class {Name}ParserException : Exception
{
    public {Name}ParserException() : base()
    {
    }

    public {Name}ParserException(string message) : base(message)
    {
    }

    public {Name}ParserException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
```

## Step 5: Create Parser Class

File: `src/MetalReleaseTracker.ParserService/Infrastructure/Parsers/{Name}Parser.cs`

### For Standard HTTP parsers (extend BaseDistributorParser):

Reference existing parsers for patterns:
- **ParagonRecordsParser** — Shopify-style site with format stripping
- **WerewolfParser** — WooCommerce with JSON-LD extraction
- **NapalmRecordsParser** — URL deduplication pattern
- **SeasonOfMistParser** — Basic structure
- **DrakkarParser** — Complex title parsing + JSON-LD

Key implementation requirements:
- Constructor: `IHtmlDocumentLoader`, `ISettingsService`, `ILogger<{Name}Parser>`
- Override all abstract members from `BaseDistributorParser`
- Use `HtmlEntity.DeEntitize()` on all extracted text
- Use `CurrentCategoryMediaType` when creating `ListingItem`
- Use `AlbumParsingHelper.GenerateSkuFromUrl()` if site has no SKU
- Use `AlbumParsingHelper.ParsePrice()` for price parsing
- Handle null XPath results gracefully
- Deduplicate URLs with `HashSet<string>`
- Convert relative URLs to absolute

### For FlareSolverr parsers:

Same as standard but will be registered differently in DI (Step 6).

### For Selenium parsers:

Implement `IListingParser` and `IAlbumDetailParser` directly.
Reference `BlackMetalVendorParser` for the Selenium pattern.
Inject `ISeleniumWebDriverFactory`. Create/dispose WebDriver per operation.

## Step 6: Register in DI

File: `src/MetalReleaseTracker.ParserService/Infrastructure/Parsers/Extensions/ParserRegistrationExtension.cs`

Add registration inside `AddParsers()` method, BEFORE the `Func<>` delegate registrations:

### Standard HTTP:
```csharp
builder.RegisterType<{Name}Parser>()
    .As<IListingParser>()
    .As<IAlbumDetailParser>()
    .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.{Name}));
```

### FlareSolverr:
```csharp
builder.RegisterType<{Name}Parser>()
    .WithParameter(new ResolvedParameter(
        (parameterInfo, context) => parameterInfo.ParameterType == typeof(IHtmlDocumentLoader),
        (parameterInfo, context) => context.Resolve<FlareSolverrHtmlDocumentLoader>()))
    .As<IListingParser>()
    .As<IAlbumDetailParser>()
    .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.{Name}));
```

### Selenium:
```csharp
builder.RegisterType<{Name}Parser>()
    .As<IListingParser>()
    .As<IAlbumDetailParser>()
    .WithMetadata<ParserMetadata>(m => m.For(meta => meta.DistributorCode, DistributorCode.{Name}));
```

## Step 7: Create Smoke Tests

File: `src/MetalReleaseTracker.ParserService/Tests/IntegrationTests/Parsers/{Name}ParserSmokeTests.cs`

```csharp
using MetalReleaseTracker.ParserService.Domain.Models.ValueObjects;
using MetalReleaseTracker.ParserService.Infrastructure.Parsers;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MetalReleaseTracker.ParserService.Tests.IntegrationTests.Parsers;

[Trait("Category", "SmokeLocal")]
public class {Name}ParserSmokeTests : ParserSmokeTestBase
{
    private const string StartUrl = "{first_category_url}";

    private {Name}Parser CreateParser()
    {
        return new {Name}Parser(
            CreateHtmlDocumentLoader(),
            CreateSettingsService(),
            NullLogger<{Name}Parser>.Instance);
    }

    [Fact]
    public async Task ParseListings_FirstPage_ReturnsNonEmptyListings()
    {
        var parser = CreateParser();
        var result = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);
        AssertListingPageValid(result);
    }

    [Fact]
    public async Task ParseListings_Pagination_ReturnsNextPageWithinSameCategory()
    {
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
        var parser = CreateParser();
        var firstPage = await parser.ParseListingsAsync(StartUrl, CancellationToken.None);
        Assert.True(firstPage.Listings.Count > 0);

        var detailUrl = firstPage.Listings[0].DetailUrl;
        var album = await parser.ParseAlbumDetailAsync(detailUrl, CancellationToken.None);
        AssertAlbumDetailValid(album, DistributorCode.{Name});
    }
}
```

## Step 8: Verify

1. Build: `dotnet build src/MetalReleaseTracker.ParserService`
2. Run smoke tests: `dotnet test src/MetalReleaseTracker.ParserService --filter "Category=SmokeLocal&FullyQualifiedName~{Name}"`
3. Fix any failing tests by adjusting selectors

## Step 9: Database Configuration

After deploying, insert a ParsingSources record:
```sql
INSERT INTO "ParsingSources" ("Id", "DistributorCode", "ParsingUrl", "IsEnabled", "CreatedAt")
VALUES (gen_random_uuid(), {enum_value}, '{first_category_url}', true, NOW());
```

Show the SQL to the user and ask for confirmation before executing.
