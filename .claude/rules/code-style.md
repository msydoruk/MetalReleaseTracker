# Code Style Rules

## Naming Conventions

- Fields: `_camelCaseWithUnderscore` (always private)
- Local variables: `camelCase`
- Constants: `PascalCase` (NOT SCREAMING_SNAKE_CASE)
- Methods: `PascalCase`
- No abbreviations: `cancellationToken` not `ct`, `exception` not `ex`, `repository` not `repo`
- Event classes: suffix with `Event` (e.g., `AlbumProcessedPublicationEvent`)
- Extension classes: suffix with `Extensions` or `Extension` (e.g., `ParserRegistrationExtension`)
- Consumer classes: suffix with `Consumer` (e.g., `AlbumProcessedEventConsumer`)
- Factory classes: suffix with `Factory` (e.g., `AlbumFactory`)

## Class Member Ordering (SA1204/SA1203)

1. Static fields
2. Static properties
3. Instance fields
4. Constructor(s)
5. Public properties
6. Public methods
7. Private/internal instance methods
8. Private static helper methods (at the very bottom)

Static members MUST appear before non-static members (StyleCop SA1203, error severity).

## Async/Await

- ALL async methods must accept `CancellationToken cancellationToken = default` as last parameter
- NEVER use `.Result` or `.Wait()` — always `await`
- NEVER use `Task.Run()` for async wrapping
- Pass cancellation token to ALL async method calls down the chain
- Use `await Task.Delay(milliseconds, cancellationToken)` for delays

## Nullable Reference Types

- Nullable reference types are enabled project-wide
- Use `T?` for nullable types, `T` for non-nullable
- Use `string.IsNullOrWhiteSpace()` not just `string.IsNullOrEmpty()`
- Don't suppress nullable warnings with `!` — fix the underlying issue

## Logging

- Inject `ILogger<T>` where T is the containing class
- Use structured logging: `_logger.LogInformation("Album added: {AlbumName}", albumName);`
- Use `{PascalCaseVariableName}` for structured log fields — NOT string interpolation
- Log levels: Debug (entry/exit), Information (business events), Warning (recoverable), Error (exceptions)
- Include context in error messages: album name, SKU, band name, URL, etc.
- Never log sensitive data (passwords, tokens, API keys)

## General

- File-scoped namespace declarations: `namespace X.Y.Z;`
- Implicit usings enabled — don't add `using System;` etc.
- Use collection expressions `[]` instead of `new List<T>()` where possible
- Use target-typed `new()` when type is obvious from context
- Prefer `string.Empty` over `""`
