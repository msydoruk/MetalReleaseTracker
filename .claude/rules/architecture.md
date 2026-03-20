# Architecture Rules

## Endpoint Structure (CoreDataService)

- Endpoints are static classes with a single `MapEndpoints(IEndpointRouteBuilder)` method in `Endpoints/` folder
- Every handler must accept `CancellationToken cancellationToken` as the last parameter
- Use `[AsParameters]` for complex filter DTOs
- Always include `.WithName()`, `.WithTags()`, and `.Produces<T>()` for OpenAPI
- Return explicit HTTP status codes: `Results.Ok()`, `Results.NotFound()`, `Results.BadRequest()`, `Results.NoContent()`
- All routes MUST be defined in `RouteConstants.cs` using nested static classes — never hardcode routes
- Route parameters must specify types: `{id:guid}`, `{name:string}`

## Service Layer

- Services contain business logic and orchestrate repositories + external services
- Entity-to-DTO mapping happens in the service layer, NOT in repositories or endpoints
- Every async method must accept `CancellationToken cancellationToken = default`
- Services return domain objects/DTOs — they should NOT know about HTTP

## Repository Layer

- Repositories are simple data access layers — no business logic
- Use `AsNoTracking()` for read-only queries
- Always call `SaveChangesAsync()` for write operations
- Return `T?` for single entity queries, `List<T>` for collections
- Use `WhereIf()` extension helper for conditional filtering
- Use `EF.Functions.ILike()` for case-insensitive PostgreSQL searches

## Consumer Pattern (MassTransit)

- Consumers implement `IConsumer<T>` with constructor-injected dependencies
- Always wrap consumer logic in try-catch, log the error, then re-throw
- Log at Information level for successful operations, Warning for skipped items, Error for exceptions
- Access event data via `context.Message`

## DI Registration

- Service registration uses extension methods on `IServiceCollection` in `ServiceExtensions/` folders
- Each extension method returns `this IServiceCollection` for chaining
- Use `AddScoped` for repositories and services (not Singleton unless stateless)
- Use `services.Configure<T>()` for `IOptions<T>` bindings
- ParserService uses Autofac (`ContainerBuilder`) for parser registration with metadata pattern
- Extensions are chained in `Program.cs` in order: config -> services -> auth -> db -> middleware

## Configuration

- ParserService: use `ISettingsService` for runtime config (DB-driven), NOT `IOptions<T>` in jobs
- CoreDataService: use `IOptions<T>` for immutable appsettings.json values
- Never use `IOptions<T>` in consumers

## Events & DTOs

- Events are plain classes for Kafka — NOT entities
- Event properties mirror entity properties for AutoMapper compatibility
- DTOs have NO methods, only properties
- Use nullable types (`string?`, `int?`) for optional fields
- Provide sensible defaults in filter DTOs (Page=1, PageSize=10)
- Never expose entities directly from endpoints
- AutoMapper: use `.Ignore()` for computed/auto-generated fields (Id, created dates)
