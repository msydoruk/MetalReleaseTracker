# Modify Existing Distributor Parser

You are modifying an existing distributor parser. This command handles: fixing broken selectors, adding new fields, changing HTML loading strategy, or adapting to website redesigns.

## Step 1: Identify the Parser

Ask the user which distributor to modify (or identify from context). Then read all related files:

1. `src/MetalReleaseTracker.ParserService/Infrastructure/Parsers/{Name}Parser.cs`
2. `src/MetalReleaseTracker.ParserService/Infrastructure/Parsers/Selectors/{Name}Selectors.cs`
3. `src/MetalReleaseTracker.ParserService/Infrastructure/Parsers/Exceptions/{Name}ParserException.cs`
4. `src/MetalReleaseTracker.ParserService/Tests/IntegrationTests/Parsers/{Name}ParserSmokeTests.cs`

## Step 2: Analyze the Current Website

Use Playwright browser tools to:
1. Navigate to the distributor's catalogue page (check `CatalogueUrls` in the parser)
2. Take snapshots of both listing and detail pages
3. Compare the live HTML structure against current selectors
4. Identify what changed or what's missing

## Step 3: Diagnose the Issue

Common issues and fixes:

### Broken Selectors (website redesign)
- Compare old XPath selectors in `{Name}Selectors.cs` with current HTML
- Update selectors to match new structure
- Add fallback selectors where elements may vary across pages
- Test both old and new product pages if possible

### Missing Fields
- Add new XPath selectors to `{Name}Selectors.cs`
- Add new parse method to `{Name}Parser.cs` (private, at bottom of class)
- Set the field in the `AlbumParsedEvent` return object
- Available fields: `Genre`, `Label`, `Press`, `Description`, `Status`, `Media`, `CanonicalTitle`, `OriginalYear`

### Price Parsing Issues
- Check currency symbol handling (strip `$`, `€`, `£`, `zł` etc.)
- Use `AlbumParsingHelper.ParsePrice()` — it expects InvariantCulture format
- Replace commas with dots: `.Replace(',', '.')`
- Extract numeric part with regex: `Regex.Match(text, @"[\d]+[.,][\d]+")`

### Pagination Broken
- Check `FindNextPageLink()` — the "next page" button may have changed
- Check `TransformNextPageUrl()` — relative vs absolute URL handling
- Verify category queue: are all `CatalogueUrls` still valid?

### Cloudflare/Anti-bot Protection Added
- If site now has Cloudflare, switch from `HtmlDocumentLoader` to `FlareSolverrHtmlDocumentLoader`
- Update DI registration in `ParserRegistrationExtension.cs` to use `ResolvedParameter`
- If site needs JavaScript rendering, consider switching to Selenium pattern

### Title Parsing Issues
- Check the title split logic (` - ` separator, en-dash `–`, em-dash `—`)
- Reference `WerewolfParser.SplitTitle()` for multi-separator handling
- Ensure format tokens are stripped (CD, LP, TAPE, DIGIPAK, etc.)

## Step 4: Implement Changes

Follow these conventions:
- Use `HtmlEntity.DeEntitize()` on all extracted text
- Handle null XPath results gracefully (return empty string, not throw)
- Use existing helpers from `AlbumParsingHelper` and `ParserHelper`
- Maintain the same code structure and member ordering as the existing parser
- If adding JSON-LD extraction, use `ParserHelper.ExtractProductJsonLd()`

## Step 5: Run Smoke Tests

```bash
dotnet test src/MetalReleaseTracker.ParserService --filter "Category=SmokeLocal&FullyQualifiedName~{Name}"
```

All three smoke tests must pass:
1. `ParseListings_FirstPage_ReturnsNonEmptyListings`
2. `ParseListings_Pagination_ReturnsNextPageWithinSameCategory`
3. `ParseAlbumDetail_SingleProduct_ReturnsPopulatedFields`

If tests fail, iterate on selector adjustments using Playwright to inspect the live HTML.

## Step 6: Build Verification

```bash
dotnet build src/MetalReleaseTracker.ParserService
```

Ensure no StyleCop or compiler warnings in modified files.
